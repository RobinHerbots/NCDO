using NCDO.Interfaces;
using System;
using System.Collections.Generic;
using System.Json;
using System.Linq;
using System.Linq.Expressions;
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

namespace NCDO
{
    /// <summary>
    /// Base CDO which returns data as Json objects
    /// </summary>
    public partial class CDO : ACloudDataObject<JsonObject, CDO_Dataset, CDO_Record>
    {
        public CDO(string respource, ICDOSession cDOSession = null, bool autoFill = false) : base(respource, cDOSession,
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
        protected D _cdoMemory;
        private string _mainTable;
        private string _primaryKey;
        private Resource _resourceDefinition;
        private Service _serviceDefinition;

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
            if (_cdoMemory != null)
            {
                _cdoMemory.Clear();
                _cdoMemory = null;
            }
        }

        public CDO_Table<R> TableReference => _cdoMemory?.Get(_mainTable) as CDO_Table<R>;

        public void AcceptChanges()
        {
            if (TableReference != null)
            {
                TableReference?.AcceptChanges();
            }
            else throw new CDOException(string.Format(Properties.Resources.API_InternalError, "AcceptChanges"));
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
            else throw new CDOException(string.Format(Properties.Resources.API_InternalError, "AddRecords"));
        }

        public void Assign(R record)
        {
            if (TableReference != null)
            {
                TableReference?.Add(record, MergeMode.Merge);
            }
            else throw new CDOException(string.Format(Properties.Resources.API_InternalError, "Assign"));
        }

        public R Create(R record)
        {
            if (TableReference != null)
            {
                TableReference?.Add(record);
                return record;
            }

            throw new CDOException(string.Format(Properties.Resources.API_InternalError, "Create"));
        }

        public void DeleteLocal()
        {
            throw new NotImplementedException();
        }

        public async Task<ICDORequest> Fill(QueryRequest queryRequest = null)
        {
            return await Read(queryRequest);
        }

        public async Task<ICDORequest> Fill(string filter)
        {
            return await Read(filter);
        }

        public async Task<ICDORequest> Fill(Expression<Func<R, bool>> filter)
        {
            return await Fill(new QueryRequest() {ABLFilter = filter.ToABLFIlter()});
        }

        /// <inheritdoc />
        public async Task<R> Find(Expression<Func<R, bool>> filter, bool autoFetch = false)
        {
            R record = null;
            if (TableReference?.JsonType == JsonType.Array)
                record = TableReference.FirstOrDefault(filter.Compile());

            if (record == null && autoFetch)
            {
                await Fill(filter);
                return await Find(filter);
            }

            return record;
        }

        /// <inheritdoc />
        public async Task<D> Get(Expression<Func<R, bool>> filter)
        {
            var read = await Read(filter);
            if (read.Success.HasValue && read.Success.Value)
            {
                var ds = new D();
                ds.Init(read.Response);
                return ds;
            }

            return null;
        }

        public async Task<R> FindById(string id, bool autoFetch = false)
        {
            var result = await Find(r => r.GetId() == id);
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

                await Fill($"{primaryKey ?? "ID"} = '{id}'");
                result = await Find(r => r.GetId() == id);
            }

            return result;
        }

        public R[] GetData()
        {
            if (TableReference != null)
            {
                return TableReference?.Cast<R>().ToArray();
            }

            throw new CDOException(string.Format(Properties.Resources.API_InternalError, "GetDate"));
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
            return _resourceDefinition.Schema;
        }

        public bool HasChanges()
        {
            if (TableReference != null)
            {
                return TableReference.IsChanged;
            }

            throw new CDOException(string.Format(Properties.Resources.API_InternalError, "HasChanges"));
        }

        public bool HasData()
        {
            if (TableReference != null)
            {
                return (TableReference?.Cast<R>()).Any();
            }

            throw new CDOException(string.Format(Properties.Resources.API_InternalError, "HasData"));
        }

        public async Task<ICDORequest> Invoke(string operation, JsonObject inputObject = null)
        {
            var operationDefinition = VerifyOperation(operation);
            //init request if needed
            var cDORequest = new CDORequest
            {
                CDO = this,
                FnName = operationDefinition.Name,
                ObjParam = _serviceDefinition.UseRequest ? new JsonObject {{"request", inputObject}} : inputObject,
                RequestUri =
                    new Uri(
                        $"{_cDOSession.ServiceURI.AbsoluteUri}{_serviceDefinition.Address}{_resourceDefinition.Path}{operationDefinition.Path}",
                        UriKind.Absolute),
                Method = new HttpMethod(operationDefinition.Verb.ToString().ToUpper())
            };

            BeforeInvoke?.Invoke(this,
                new CDOEventArgs<T, D, R> {CDO = this, Request = cDORequest, Session = _cDOSession});
            await DoRequest(cDORequest, ProcessInvokeResponse);
            AfterInvoke?.Invoke(this,
                new CDOEventArgs<T, D, R> {CDO = this, Request = cDORequest, Session = _cDOSession});

            return cDORequest;
        }

        public async Task<ICDORequest> Read(QueryRequest queryRequest = null)
        {
            return await Read(queryRequest?.ToString(_resourceDefinition.Operations
                .FirstOrDefault(o => o.Type == OperationType.Read)?.Capabilities));
        }

        public async Task<ICDORequest> Read(string filter)
        {
            var operationDefinition = VerifyOperation(null, OperationType.Read);

            //setup filter for get request
            var filterpath = string.IsNullOrEmpty(filter)
                ? ""
                : operationDefinition.Path.ToString().Replace("{filter}", filter);

            //init request if needed
            var cDORequest = new CDORequest
            {
                CDO = this,
                FnName = operationDefinition.Name,
                RequestUri =
                    new Uri(
                        $"{_cDOSession.ServiceURI.AbsoluteUri}{_serviceDefinition.Address}{_resourceDefinition.Path}{filterpath}",
                        UriKind.Absolute),
                Method = new HttpMethod(operationDefinition.Verb.ToString().ToUpper())
            };

            BeforeFill?.Invoke(this,
                new CDOEventArgs<T, D, R> {CDO = this, Request = cDORequest, Session = _cDOSession});
            BeforeRead?.Invoke(this,
                new CDOEventArgs<T, D, R> {CDO = this, Request = cDORequest, Session = _cDOSession});
            await DoRequest(cDORequest, ProcessCRUDResponse);
            AfterFill?.Invoke(this,
                new CDOEventArgs<T, D, R> {CDO = this, Request = cDORequest, Session = _cDOSession});
            AfterRead?.Invoke(this,
                new CDOEventArgs<T, D, R> {CDO = this, Request = cDORequest, Session = _cDOSession});

            return cDORequest;
        }

        public async Task<ICDORequest> Read(Expression<Func<R, bool>> filter)
        {
            return await Read(new QueryRequest() {ABLFilter = filter.ToABLFIlter()});
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
            else throw new CDOException(string.Format(Properties.Resources.API_InternalError, "RejectChanges"));
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

            throw new CDOException(string.Format(Properties.Resources.API_InternalError, "Remove"));
        }

        public async Task SaveChanges(CDO_Table<R> tableRef = null)
        {
            if (tableRef == null) tableRef = TableReference;
            BeforeSaveChanges?.Invoke(this, new CDOEventArgs<T, D, R> {CDO = this, Request = null});

            if (tableRef != null)
            {
                Operation operation = null;
                //delete
                if (tableRef._deleted.Any())
                {
                    operation = VerifyOperation(null, OperationType.Delete);
                    var deleteRequest = new CDORequest
                    {
                        CDO = this,
                        FnName = operation.Name,
                        RequestUri =
                            new Uri(
                                $"{_cDOSession.ServiceURI.AbsoluteUri}{_serviceDefinition.Address}{_resourceDefinition.Path}",
                                UriKind.Absolute),
                        Method = new HttpMethod(operation.Verb.ToString().ToUpper()),
                        ObjParam = new D {{_mainTable, new CDO_Table<R>(tableRef._deleted)}}
                    };
                    BeforeDelete?.Invoke(this,
                        new CDOEventArgs<T, D, R> {CDO = this, Request = deleteRequest, Session = _cDOSession});
                    await DoRequest(deleteRequest, ProcessCRUDResponse);
                    AfterDelete?.Invoke(this,
                        new CDOEventArgs<T, D, R> {CDO = this, Request = deleteRequest, Session = _cDOSession});
                }

                //create
                if (tableRef._new.Any())
                {
                    operation = VerifyOperation(null, OperationType.Create);
                    var createRequest = new CDORequest
                    {
                        CDO = this,
                        FnName = operation.Name,
                        RequestUri =
                            new Uri(
                                $"{_cDOSession.ServiceURI.AbsoluteUri}{_serviceDefinition.Address}{_resourceDefinition.Path}",
                                UriKind.Absolute),
                        Method = new HttpMethod(operation.Verb.ToString().ToUpper()),
                        ObjParam = new D {{_mainTable, new CDO_Table<R>(tableRef._new)}}
                    };
                    BeforeCreate?.Invoke(this,
                        new CDOEventArgs<T, D, R> {CDO = this, Request = createRequest, Session = _cDOSession});
                    await DoRequest(createRequest, ProcessCRUDResponse);
                    AfterCreate?.Invoke(this,
                        new CDOEventArgs<T, D, R> {CDO = this, Request = createRequest, Session = _cDOSession});
                }

                //update
                if (tableRef._changed.Any())
                {
                    operation = VerifyOperation(null, OperationType.Update);
                    var updateRequest = new CDORequest
                    {
                        CDO = this,
                        FnName = operation.Name,
                        RequestUri =
                            new Uri(
                                $"{_cDOSession.ServiceURI.AbsoluteUri}{_serviceDefinition.Address}{_resourceDefinition.Path}",
                                UriKind.Absolute),
                        Method = new HttpMethod(operation.Verb.ToString().ToUpper()),
                        ObjParam = new D {{_mainTable, new CDO_Table<R>(tableRef._changed)}}
                    };
                    BeforeUpdate?.Invoke(this,
                        new CDOEventArgs<T, D, R> {CDO = this, Request = updateRequest, Session = _cDOSession});
                    await DoRequest(updateRequest, ProcessCRUDResponse);
                    AfterUpdate?.Invoke(this,
                        new CDOEventArgs<T, D, R> {CDO = this, Request = updateRequest, Session = _cDOSession});
                }

                //all done => accept the changes
                if (AutoApplyChanges) tableRef.AcceptChanges();
            }
            else throw new CDOException(string.Format(Properties.Resources.API_InternalError, "SaveChanges"));

            AfterSaveChanges?.Invoke(this, new CDOEventArgs<T, D, R> {CDO = this, Request = null});
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
            Func<HttpResponseMessage, CDORequest, Task> processResponse)
        {
            using (var request = new HttpRequestMessage())
            {
                //add authentication headers
                _cDOSession.OnOpenRequest(_cDOSession.HttpClient, request);

                //init request from CDORequest
                request.Method = cDORequest.Method;
                request.RequestUri = cDORequest.RequestUri;
                if (!cDORequest.Method.Equals(HttpMethod.Get))
                    request.Content =
                        new StringContent(cDORequest.ObjParam.ToString(), Encoding.UTF8, "application/json");
                using (var response = await _cDOSession.HttpClient.SendAsync(request,
                    HttpCompletionOption.ResponseHeadersRead, default(CancellationToken)))
                {
                    await processResponse?.Invoke(response, cDORequest);
                }
            }

            return cDORequest;
        }

        public virtual async Task ProcessInvokeResponse(HttpResponseMessage response, CDORequest request)
        {
            request.Success = response.IsSuccessStatusCode;
            request.ResponseMessage = response;
            //https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Transfer-Encoding
            if ((response.Headers.TransferEncodingChunked.HasValue && response.Headers.TransferEncodingChunked.Value) ||
                response.Content.Headers.ContentLength > 0)
            {
                using (var dataStream = await response.Content.ReadAsStreamAsync())
                {
                    if (dataStream != null)
                    {
                        var parsedResponse = JsonValue.Load(dataStream);
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

            request.ThrowOnError();
        }

        public virtual async Task ProcessCRUDResponse(HttpResponseMessage response, CDORequest request)
        {
            await ProcessInvokeResponse(response, request);
            if (request.Success.HasValue && request.Success.Value)
                if (request.Method == HttpMethod.Get || request.Method == HttpMethod.Post ||
                    request.Method == HttpMethod.Put)
                {
                    if (request.Method == HttpMethod.Post)
                    {
                        foreach (R r in TableReference.New)
                        {
                            TableReference.Remove(r);
                        }
                    }

                    if (_cdoMemory == null)
                    {
                        //init cdoMemory
                        _cdoMemory = new D();
                    }

                    _cdoMemory.Init(request.Response);
                }
        }

        #endregion

        #region private 

        private void InitCDO(bool autoFill)
        {
            VerifyResourceName(Name);
            DetermineMainTable();
            if (autoFill)
            {
                Fill().Wait();
            }
        }

        /// <summary>
        ///     Verify if the resource is available and return the catalog definition for the catalog
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        private Resource VerifyResourceName(string resource)
        {
            _serviceDefinition =
                _cDOSession.Services.FirstOrDefault(s => s.Resources.Any(r => r.Name.Equals(resource)));
            var resourceDefinition = _serviceDefinition?.Resources.FirstOrDefault(r => r.Name.Equals(resource));
            if (resourceDefinition == null)
                throw new NotSupportedException($"Invalid resourcename {resource}.");
            return _resourceDefinition = resourceDefinition;
        }

        private Operation VerifyOperation(string operation, OperationType operationType = OperationType.Invoke)
        {
            var operationDefinition = _resourceDefinition.Operations.FirstOrDefault(o =>
                o.Type == operationType && (string.IsNullOrEmpty(operation) || o.Name.Equals(operation)));
            if (operationDefinition == null)
                throw new NotSupportedException($"Invalid {operationType} operation {operation}.");
            return operationDefinition;
        }

        private void DetermineMainTable()
        {
            _mainTable = _resourceDefinition.Relations != null
                ? _resourceDefinition.Relations.FirstOrDefault().ParentName
                : _resourceDefinition.Schema?.Properties.FirstOrDefault().Value.Properties.FirstOrDefault().Key;
            _primaryKey = _resourceDefinition.Schema?.Properties.FirstOrDefault().Value.Properties[_mainTable]
                .PrimaryKey.FirstOrDefault();
        }

        #endregion
    }
}