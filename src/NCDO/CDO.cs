using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Json;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NCDO.Catalog;
using NCDO.CDOMemory;
using NCDO.Definitions;
using NCDO.Events;
using NCDO.Extensions;
using NCDO.Interfaces;
using NCDO.Properties;

namespace NCDO
{
    /// <summary>
    /// Base CDO which returns data as Json objects
    /// </summary>
    public partial class CDO : ACloudDataObject<JsonObject, CDO_Dataset, CDO_Record>
    {
        public CDO(string resource, ICDOSession cDOSession = null, bool autoFill = false) : base(resource, cDOSession,
            autoFill)
        {
        }
    }


    /// <summary>
    /// Abstract implementation of ICloudDataObject
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="D"></typeparam>
    /// <typeparam name="R"></typeparam>
    public abstract class ACloudDataObject<T, D, R> : ICloudDataObject<T, D, R>
        where T : class
        where D : CDO_Dataset, new()
        where R : CDO_Record, new()
    {
        private readonly ICDOSession _cDOSession;
        protected D _cdoMemory = new D();
        private string _mainTable;
        private (Service service, Resource resource) _catalogDefinition;
        private static SemaphoreSlim _requestMutex = new SemaphoreSlim(1, 1);
        private static SemaphoreSlim _saveChangesMutex = new SemaphoreSlim(1, 1);

        /// <summary>
        ///     CDO Constructor
        /// </summary>
        /// <param name="resource">
        ///     A string expression set to the name of a resource provided by a Cloud Data Service for
        ///     which a login session has been started.
        /// </param>
        /// <param name="autoFill">
        ///     Specifies whether the CDO invokes its fill( ) method upon instantiation to initialize CDO memory with data from the
        ///     resource.
        /// </param>
        protected ACloudDataObject(string resource, ICDOSession cDOSession = null, bool autoFill = false)
        {
            _cDOSession = cDOSession;
            if (_cDOSession == null)
            {
                //pickup last initialized session
                _cDOSession = CDOSession.Instance;
                if (_cDOSession == null)
                    throw new Exception("No session available");
            }

            Name = resource;

            InitCDO(autoFill);
        }


        public bool AutoApplyChanges { get; set; } = true;
        public bool AutoSort { get; set; }
        public bool CaseSensitive { get; set; }

        public string Name { get; }
        public bool UseRelationships { get; set; }

        public R Record { get; set; }

        /// <inheritdoc />
        public void Reset()
        {
            _cdoMemory?.Clear();
        }

        public CDO_Table<R> TableReference => _cdoMemory?.Get(_mainTable) as CDO_Table<R>;

        public void AcceptChanges()
        {
            if (TableReference != null)
            {
                TableReference?.AcceptChanges();
            }
            else throw new CDOException(string.Format(Resources.API_InternalError, "AcceptChanges"));
        }

        //public bool AcceptRowChanges()
        //{
        //    throw new  NotImplementedException();
        //}

        public R Add(R record)
        {
            return Create(record);
        }

        public void AddLocalRecords()
        {
            throw new NotImplementedException();
        }

        public void AddRecords(IEnumerable<R> records, MergeMode mergeMode = MergeMode.Replace)
        {
            if (TableReference != null)
            {
                TableReference?.AddRange(records);
            }
            else throw new CDOException(string.Format(Resources.API_InternalError, "AddRecords"));
        }

        public void Assign(R record)
        {
            if (TableReference != null)
            {
                TableReference?.Add(record, MergeMode.Merge);
            }
            else throw new CDOException(string.Format(Resources.API_InternalError, "Assign"));
        }

        public R Create(R record)
        {
            if (TableReference != null)
            {
                TableReference?.Add(record);
                return record;
            }

            throw new CDOException(string.Format(Resources.API_InternalError, "Create"));
        }

        public void DeleteLocal()
        {
            throw new NotImplementedException();
        }

        public async Task<ICDORequest> Fill(QueryRequest queryRequest = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Read(queryRequest, cancellationToken);
        }

        public async Task<ICDORequest> Fill(string filter,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Read(filter, cancellationToken);
        }

        public async Task<ICDORequest> Fill(Expression<Func<R, bool>> filter,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Fill(new QueryRequest() {ABLFilter = filter.ToABLFIlter()}, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<R> Find(Expression<Func<R, bool>> filter, bool autoFetch = false, bool children = true,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            R record = null;
            if (TableReference?.JsonType == JsonType.Array)
                record = TableReference.FirstOrDefault(filter.Compile());

            if (record == null && autoFetch)
            {
                await Fill(new QueryRequest(){Children = children}.Filter(filter), cancellationToken);
                return await Find(filter, cancellationToken: cancellationToken);
            }

            return record;
        }

        /// <inheritdoc />
        public async Task<D> Get(Expression<Func<R, bool>> filter = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Get(new QueryRequest() {ABLFilter = filter?.ToABLFIlter()}, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<D> Get(QueryRequest request,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
//            BeforeRead += ACloudDataObject_BeforeRead;
            var read = await Read(request, cancellationToken);
//            BeforeRead -= ACloudDataObject_BeforeRead;

            if (read.Success.HasValue && read.Success.Value)
            {
                var ds = new D();
                ds.Init(read.Response);
                return ds;
            }

            return null;
        }

        private void ACloudDataObject_BeforeRead(object sender, CDOEventArgs<T, D, R> e)
        {
            e.Request.CdoMemory = false;
        }

        public async Task<R> FindById(string id, bool autoFetch = false,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            var result = await Find(r => r.GetId() == id, cancellationToken: cancellationToken);
            if (result == null)
            {
                var defaultsFieldInfo = typeof(R).GetField("_defaults",
                    BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                var pkFieldInfo = typeof(R).GetField("primaryKey",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                var primaryKey = (string) pkFieldInfo.GetValue(defaultsFieldInfo.GetValue(null));
                if (string.IsNullOrEmpty(primaryKey))
                {
                    new R();
                    primaryKey = (string) pkFieldInfo.GetValue(defaultsFieldInfo.GetValue(null));
                }

                await Fill($"{primaryKey ?? "ID"} = '{id}'", cancellationToken);
                result = await Find(r => r.GetId() == id, cancellationToken: cancellationToken);
            }

            return result;
        }

        public R[] GetData()
        {
            if (TableReference != null)
            {
                return Enumerable.ToArray<R>(TableReference);
            }

            throw new CDOException(string.Format(Resources.API_InternalError, "GetDate"));
        }

        public string[] GetErrors()
        {
            throw new NotImplementedException();
        }

        public string GetErrorString()
        {
            throw new NotImplementedException();
        }

        public string GetId()
        {
            throw new NotImplementedException();
        }

        public Schema GetSchema()
        {
            return _catalogDefinition.resource.Schema;
        }

        public bool HasChanges()
        {
            if (TableReference != null)
            {
                return TableReference.IsChanged;
            }

            throw new CDOException(string.Format(Resources.API_InternalError, "HasChanges"));
        }

        public bool HasData()
        {
            return TableReference?.Count > 0;
        }

        public async Task<ICDORequest> Invoke(string operation, JsonObject inputObject = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            var operationDefinition = _cDOSession.VerifyOperation(Name, operation);
            //init request if needed
            var cDORequest = new CDORequest
            {
                InvokeOperation = true,
                CDO = this,
                FnName = operationDefinition.Name,
                ObjParam = _catalogDefinition.service.UseRequest
                    ? new JsonObject {{"request", inputObject}}
                    : inputObject,
                RequestUri =
                    new Uri(
                        $"{_cDOSession.ServiceURI.AbsoluteUri}{_catalogDefinition.service.Address}{_catalogDefinition.resource.Path}{operationDefinition.Path}",
                        UriKind.Absolute),
                Method = new HttpMethod(operationDefinition.Verb.ToString().ToUpper())
            };

            BeforeInvoke?.Invoke(this,
                new CDOEventArgs<T, D, R> {CDO = this, Request = cDORequest, Session = _cDOSession});
            await DoRequest(cDORequest, cancellationToken);
            AfterInvoke?.Invoke(this,
                new CDOEventArgs<T, D, R> {CDO = this, Request = cDORequest, Session = _cDOSession});

            return cDORequest;
        }

        public async Task<ICDORequest> Read(QueryRequest queryRequest = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Read(queryRequest?.ToString(_catalogDefinition.resource.Operations
                .FirstOrDefault(o => o.Type == OperationType.Read)?.Capabilities), cancellationToken);
        }

        public async Task<ICDORequest> Read(string filter,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            var operationDefinition = _cDOSession.VerifyOperation(Name, null, OperationType.Read);

            //setup filter for get request
            var filterpath = string.IsNullOrEmpty(filter)
                ? ""
                : operationDefinition.Path.ToString().Replace("{filter}", WebUtility.UrlEncode(filter));

            //init request if needed
            var cDORequest = new CDORequest
            {
                CDO = this,
                FnName = operationDefinition.Name,
                RequestUri =
                    new Uri(
                        $"{_cDOSession.ServiceURI.AbsoluteUri}{_catalogDefinition.service.Address}{_catalogDefinition.resource.Path}{filterpath}",
                        UriKind.Absolute),
                Method = new HttpMethod(operationDefinition.Verb.ToString().ToUpper())
            };

            BeforeFill?.Invoke(this,
                new CDOEventArgs<T, D, R> {CDO = this, Request = cDORequest, Session = _cDOSession});
            BeforeRead?.Invoke(this,
                new CDOEventArgs<T, D, R> {CDO = this, Request = cDORequest, Session = _cDOSession});
            await DoRequest(cDORequest, cancellationToken);
            AfterFill?.Invoke(this,
                new CDOEventArgs<T, D, R> {CDO = this, Request = cDORequest, Session = _cDOSession});
            AfterRead?.Invoke(this,
                new CDOEventArgs<T, D, R> {CDO = this, Request = cDORequest, Session = _cDOSession});

            return cDORequest;
        }

        public async Task<ICDORequest> Read(Expression<Func<R, bool>> filter,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Read(new QueryRequest() {ABLFilter = filter?.ToABLFIlter()}, cancellationToken);
        }

        public void ReadLocal()
        {
            throw new NotImplementedException();
        }

        public void RejectChanges()
        {
            if (TableReference != null)
            {
                TableReference.RejectChanges();
            }
            else throw new CDOException(string.Format(Resources.API_InternalError, "RejectChanges"));
        }

        //public void RejectRowChanges()
        //{
        //    throw new NotImplementedException();
        //}

        public bool Remove(R record)
        {
            if (TableReference != null)
            {
                return TableReference.Remove(record);
            }

            throw new CDOException(string.Format(Resources.API_InternalError, "Remove"));
        }

        public async Task SaveChanges(CDO_Table<R> tableRef = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            await _saveChangesMutex.WaitAsync(default(CancellationToken));
            try
            {
                if (tableRef == null) tableRef = TableReference;
                tableRef.NegateNewIds();
                BeforeSaveChanges?.Invoke(this, new CDOEventArgs<T, D, R> {CDO = this, Request = null});

                Operation operation = null;
                //delete
                if (tableRef._deleted.Any())
                {
                    operation = _cDOSession.VerifyOperation(Name, null, OperationType.Delete);
                    var deleteRequest = new CDORequest
                    {
                        CDO = this,
                        FnName = operation.Name,
                        RequestUri =
                            new Uri(
                                $"{_cDOSession.ServiceURI.AbsoluteUri}{_catalogDefinition.service.Address}{_catalogDefinition.resource.Path}",
                                UriKind.Absolute),
                        Method = new HttpMethod(operation.Verb.ToString().ToUpper()),
                        ObjParam = new D {{_mainTable, new CDO_Table<R>(tableRef._deleted.Values)}}
                    };
                    BeforeDelete?.Invoke(this,
                        new CDOEventArgs<T, D, R> {CDO = this, Request = deleteRequest, Session = _cDOSession});
                    await DoRequest(deleteRequest, cancellationToken);
                    AfterDelete?.Invoke(this,
                        new CDOEventArgs<T, D, R> {CDO = this, Request = deleteRequest, Session = _cDOSession});
                }

                //create
                if (tableRef._new.Any())
                {
                    operation = _cDOSession.VerifyOperation(Name, null, OperationType.Create);
                    var createRequest = new CDORequest
                    {
                        CDO = this,
                        FnName = operation.Name,
                        RequestUri =
                            new Uri(
                                $"{_cDOSession.ServiceURI.AbsoluteUri}{_catalogDefinition.service.Address}{_catalogDefinition.resource.Path}",
                                UriKind.Absolute),
                        Method = new HttpMethod(operation.Verb.ToString().ToUpper()),
                        ObjParam = new D {{_mainTable, new CDO_Table<R>(tableRef._new.Values)}}
                    };
                    BeforeCreate?.Invoke(this,
                        new CDOEventArgs<T, D, R> {CDO = this, Request = createRequest, Session = _cDOSession});
                    await DoRequest(createRequest, cancellationToken);
                    AfterCreate?.Invoke(this,
                        new CDOEventArgs<T, D, R> {CDO = this, Request = createRequest, Session = _cDOSession});
                }

                //update
                if (tableRef._changed.Any())
                {
                    operation = _cDOSession.VerifyOperation(Name, null, OperationType.Update);
                    var updateRequest = new CDORequest
                    {
                        CDO = this,
                        FnName = operation.Name,
                        RequestUri =
                            new Uri(
                                $"{_cDOSession.ServiceURI.AbsoluteUri}{_catalogDefinition.service.Address}{_catalogDefinition.resource.Path}",
                                UriKind.Absolute),
                        Method = new HttpMethod(operation.Verb.ToString().ToUpper()),
                        ObjParam = new D {{_mainTable, new CDO_Table<R>(tableRef._changed.Values)}}
                    };
                    BeforeUpdate?.Invoke(this,
                        new CDOEventArgs<T, D, R> {CDO = this, Request = updateRequest, Session = _cDOSession});
                    await DoRequest(updateRequest, cancellationToken);
                    AfterUpdate?.Invoke(this,
                        new CDOEventArgs<T, D, R> {CDO = this, Request = updateRequest, Session = _cDOSession});
                }

                //all done => accept the changes
                if (AutoApplyChanges) tableRef.AcceptChanges();

                AfterSaveChanges?.Invoke(this, new CDOEventArgs<T, D, R> {CDO = this, Request = null});
            }
            finally
            {
                _saveChangesMutex.Release();
            }
        }

        public void SaveLocal()
        {
            throw new NotImplementedException();
        }

        public void SetSortFields()
        {
            throw new NotImplementedException();
        }

        public void SetSortFn()
        {
            throw new NotImplementedException();
        }

        public void Sort()
        {
            throw new NotImplementedException();
        }

        #region Events

        public event EventHandler<CDOEventArgs<T, D, R>> AfterCreate;
        public event EventHandler<CDOEventArgs<T, D, R>> AfterDelete;
        public event EventHandler<CDOEventArgs<T, D, R>> AfterFill;
        public event EventHandler<CDOEventArgs<T, D, R>> AfterInvoke;
        public event EventHandler<CDOEventArgs<T, D, R>> AfterRead;
        public event EventHandler<CDOEventArgs<T, D, R>> AfterSaveChanges;
        public event EventHandler<CDOEventArgs<T, D, R>> AfterUpdate;
        public event EventHandler<CDOEventArgs<T, D, R>> BeforeCreate;
        public event EventHandler<CDOEventArgs<T, D, R>> BeforeDelete;
        public event EventHandler<CDOEventArgs<T, D, R>> BeforeFill;
        public event EventHandler<CDOEventArgs<T, D, R>> BeforeInvoke;
        public event EventHandler<CDOEventArgs<T, D, R>> BeforeRead;
        public event EventHandler<CDOEventArgs<T, D, R>> BeforeSaveChanges;
        public event EventHandler<CDOEventArgs<T, D, R>> BeforeUpdate;

        #endregion

        #region Request logic

        public virtual async Task<ICDORequest> DoRequest(CDORequest cDORequest,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (_cDOSession.LoginResult != SessionStatus.AUTHENTICATION_SUCCESS &&
                _cDOSession.Options.AuthenticationModel == AuthenticationModel.Bearer_OnBehalf)
            {
                await _cDOSession.Login(cancellationToken);
            }

            if (_cDOSession.LoginResult != SessionStatus.AUTHENTICATION_SUCCESS)
                throw new CDOException(_cDOSession.LoginResult.ToString(),
                    _cDOSession.Connected ? Resources.Session_NotAuthenticated : Resources.Session_NoNetwork);


            if (cDORequest.Method == HttpMethod.Get)
            {
                //wait for savechanges to complete before launching a read.
                await _saveChangesMutex.WaitAsync(cancellationToken);
                _saveChangesMutex.Release();
            }

            await _requestMutex.WaitAsync(cancellationToken);
            try
            {
                using (var request = new HttpRequestMessage())
                {
                    //add authentication headers
                    await _cDOSession.OnOpenRequest(_cDOSession.HttpClient, request);

                    //init request from CDORequest
                    request.Method = cDORequest.Method;
                    request.RequestUri = cDORequest.RequestUri;
                    if (!cDORequest.Method.Equals(HttpMethod.Get))
                        request.Content =
                            new StringContent(cDORequest.ObjParam.ToString(), Encoding.UTF8, "application/json");
                    using (var response = await _cDOSession.HttpClient.SendAsync(request,
                        HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                    {
                        await ProcessResponse(response, cDORequest, cancellationToken);
                    }
                }

                return cDORequest;
            }
            finally
            {
                _requestMutex.Release();
            }
        }

        public virtual async Task ProcessResponse(HttpResponseMessage response, CDORequest request,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            request.Success = response.IsSuccessStatusCode;
            request.ResponseMessage = response;
            //https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Transfer-Encoding
            if ((response.Headers.TransferEncodingChunked.HasValue && response.Headers.TransferEncodingChunked.Value) ||
                response.Content.Headers.ContentLength > 0)
            {
                using (var dataStream = await response.Content.ReadAsStreamAsync())
                {
                    {
                        if (dataStream != null)
                        {
                            var parsedResponse = JsonValue.Load(new StreamReader(dataStream, true));
                            if (parsedResponse != null)
                            {
                                request.Response =
                                    (JsonObject) (!string.IsNullOrEmpty(request.FnName) &&
                                                  parsedResponse.ContainsKey("response")
                                        ? parsedResponse.Get("response")
                                        : parsedResponse);
                            }
                        }
                    }
                }
            }

            if (!request.InvokeOperation && request.Success.HasValue && request.Success.Value)
                if (request.Method == HttpMethod.Post)
                {
                    if (TableReference.New != null)
                        for (var ndx = 0; ndx < TableReference.New.Count; ndx++)
                        {
                            var r = TableReference.New[TableReference.New.Keys.ToList()[ndx]];
                            try //try to recover the new ID
                            {
                                //var primaryKey = r.primaryKey ?? "ID";
                                //r.Set(primaryKey, request.Response.Get(typeof(D).Name).Get("tt" + Name)[ndx]?.Get(primaryKey));

                                TableReference.Merge(r,
                                    request.Response.Get(typeof(D).Name).Get("tt" + Name)[ndx] as JsonObject);
                            }
                            catch
                            {
                            }
                        }
                }
                else if (request.Method == HttpMethod.Get && request.CdoMemory)
                {
                    _cdoMemory.Init(request.Response);
                }

            request.ThrowOnError();
        }

        #endregion

        #region private 

        private void InitCDO(bool autoFill, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            _catalogDefinition = _cDOSession.VerifyResourceName(Name);
            _mainTable = _cDOSession.DetermineMainTable(Name);
            if (autoFill)
            {
                Fill(cancellationToken: cancellationToken).Wait(cancellationToken);
            }
            else _cdoMemory.Init();
        }

        #endregion

        #region IDisposable Support

        ~ACloudDataObject()
        {
            Dispose(false);
        }

        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing || this._disposed)
                return;

            _requestMutex?.Dispose();
            _saveChangesMutex?.Dispose();

            _disposed = true;
        }

        /// <summary>Throws if this class has been disposed.</summary>
        protected void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(this.GetType().Name);
        }

        #endregion
    }
}