using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace NCDO.Interfaces
{
    /// <summary>
    ///     The CDO provides access to resources in a Cloud Data Service, known as a Cloud Data Resource.
    /// </summary>
    public interface ICloudDataObject
    {
        #region Properties

        /// <summary>
        ///     A Boolean on a CDO that indicates if the CDO
        ///     automatically accepts or rejects changes to CDO memory
        ///     when you call the saveChanges( ) method.
        /// </summary>
        bool AutoApplyChanges { get; set; }

        /// <summary>
        ///     A Boolean on a CDO and its table references that
        ///     indicates if record objects are sorted automatically on the
        ///     affected table references in CDO memory at the completion
        ///     of a supported CDO operation.
        /// </summary>
        bool AutoSort { get; set; }

        /// <summary>
        ///     A Boolean on a CDO and its table references that
        ///     indicates if String field comparisons performed by
        ///     supported CDO operations are case sensitive or
        ///     case-insensitive for the affected table references in CDO
        ///     memory
        /// </summary>
        bool CaseSensitive { get; set; }

        /// <summary>
        ///     The name of the resource for which the current CDO is
        ///     created.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     A Boolean that specifies whether CDO methods that
        ///     operate on table references in CDO memory work with the
        ///     table relationships defined in the schema (that is, work only
        ///     on the records of a child table that are related to the parent).
        /// </summary>
        bool UseRelationships { get; set; }

        #endregion
        #region Methods

        /// <summary>
        ///     Accepts changes to the data in CDO memory for the
        ///     specified table reference or for all table references of the
        ///     specified CDO.
        /// </summary>
        void AcceptChanges();

        /// <summary>
        ///     Accepts changes to the data in CDO memory for a specified
        ///     record object.
        /// </summary>
        //bool AcceptRowChanges();

        /// <summary>
        ///     Reads the record objects stored in the specified local storage
        ///     area and updates CDO memory based on these record
        ///     objects, including any pending changes and before-image
        ///     data, if they exist.
        /// </summary>
        void AddLocalRecords();
        /// <summary>
        ///     Clears out all data and changes stored in a specified local
        ///     storage area, and removes the cleared storage area.
        /// </summary>
        void DeleteLocal();

        /// <summary>
        ///     Initializes CDO memory with record objects from the data
        ///     records in a single table, or in one or more tables of a
        ///     ProDataSet, as returned by the built-in read operation of the
        ///     resource for which the CDO is created.
        /// </summary>
        Task<ICDORequest> Fill(QueryRequest queryRequest = null);
        /// <summary>
        ///     Initializes CDO memory with record objects from the data
        ///     records in a single table, or in one or more tables of a
        ///     ProDataSet, as returned by the built-in read operation of the
        ///     resource for which the CDO is created.
        /// </summary>
        Task<ICDORequest> Fill(string filter);


        /// <summary>
        ///     Returns an array of errors from the most recent CDO
        ///     operation.
        /// </summary>
        /// <returns></returns>
        string[] GetErrors();

        /// <summary>
        ///     Returns any before-image error string in the data of a record
        ///     object referenced in CDO memory that was set as the result
        ///     of a Data Object create, update, delete, or submit
        ///     operation.
        /// </summary>
        /// <returns></returns>
        string GetErrorString();

        /// <summary>
        ///     Returns the unique internal ID for the record object
        ///     referenced in CDO memory.
        /// </summary>
        /// <returns></returns>
        string GetId();

        /// <summary>
        ///     Returns an array of objects, one for each field that defines
        ///     the schema of a table referenced in CDO memory.
        /// </summary>
        Schema GetSchema();

        /// <summary>
        ///     Returns true if record objects can be found in any of the
        ///     tables referenced in CDO memory (with or without pending
        ///     changes), or in only the single table referenced on the CDO,
        ///     depending on how the method is called; and returns false
        ///     if no record objects are found in either case.
        /// </summary>
        /// <returns></returns>
        bool HasData();

        /// <summary>
        ///     Returns true if CDO memory contains any pending
        ///     changes (with or without before-image data), and returns
        ///     false if CDO memory has no pending changes.
        /// </summary>
        /// <returns></returns>
        bool HasChanges();

        /// <summary>
        ///     Asynchronously calls a custom invocation method on the
        ///     CDO to execute an Invoke operation defined by a Data
        ///     Object resource.
        /// </summary>
        Task<ICDORequest> Invoke(string operation, JsonObject inputObject = null);

        /// <summary>
        ///     Initializes CDO memory with record objects from the data
        ///     records in a single table, or in one or more tables of a
        ///     ProDataSet, as returned by the built-in read operation of the
        ///     resource for which the CDO is created.
        /// </summary>
        Task<ICDORequest> Read(QueryRequest queryRequest = null);
        /// <summary>
        ///     Initializes CDO memory with record objects from the data
        ///     records in a single table, or in one or more tables of a
        ///     ProDataSet, as returned by the built-in read operation of the
        ///     resource for which the CDO is created.
        /// </summary>
        Task<ICDORequest> Read(string filter);

        /// <summary>
        ///     Clears out the data in CDO memory and replaces it with all
        ///     the data stored in a specified local storage area, including
        ///     any pending changes and before-image data, if they exist.
        /// </summary>
        void ReadLocal();

        /// <summary>
        ///     Rejects changes to the data in CDO memory for the
        ///     specified table reference or for all table references of the
        ///     specified CDO.
        /// </summary>
        void RejectChanges();

        ///// <summary>
        /////     Rejects changes to the data in CDO memory for a specified
        /////     record object.
        ///// </summary>
        //void RejectRowChanges();



        /// <summary>
        ///     Synchronizes to the Cloud Data Server all changes pending in
        ///     CDO memory since the last call to the fill( ) or
        ///     saveChanges( ) methods, or since any prior changes
        ///     have been otherwise accepted or rejected.
        /// </summary>
        Task SaveChanges();

        /// <summary>
        ///     Saves CDO memory to a specified local storage area,
        ///     including pending changes and any before-image data,
        ///     according to a specified data mode.
        /// </summary>
        void SaveLocal();

        /// <summary>
        ///     Specifies or clears the record fields on which to automatically
        ///     sort the record objects for a table reference after you have
        ///     set its autoSort property to true (the default).
        /// </summary>
        void SetSortFields();

        /// <summary>
        ///     Specifies or clears a user-defined sort function with which
        ///     to automatically sort the record objects for a table reference
        ///     after you have set its autoSort property to true (the
        ///     default).
        /// </summary>
        void SetSortFn();

        /// <summary>
        ///     Sorts the existing record objects for a table reference in
        ///     CDO memory using either specified sort fields or a specified
        ///     user-defined sort function.
        /// </summary>
        void Sort();



        Task ProcessCRUDResponse(HttpResponseMessage response, CDORequest request);
        Task ProcessInvokeResponse(HttpResponseMessage response, CDORequest request);

        #endregion
    }

    /// <summary>
    ///     The CDO provides access to resources in a Cloud Data Service, known as a Cloud Data Resource.
    /// </summary>
    public interface ICloudDataObject<T, D, R> : ICloudDataObject
        where T : class
        where D : CDO_Dataset, new()
        where R : CDO_Record, new()
    {

        #region Methods
        /// <summary>
        ///     Creates a new record object for a table referenced in CDO
        ///     memory and returns a reference to the new record.
        /// </summary>
        R Add(R record);
        /// <summary>
        ///     Reads an array, table, or ProDataSet object containing one
        ///     or more record objects and updates CDO memory based
        ///     on these record objects, including any pending changes and
        ///     before-image data, if they exist.
        /// </summary>
        void AddRecords(IEnumerable<R> records, MergeMode mergeMode = MergeMode.Replace);
        /// <summary>
        ///     Updates field values for the specified CloudDataRecord
        ///     object in CDO memory.
        /// </summary>
        void Assign(R record);
        /// <summary>
        ///     Creates a new record object for a table referenced in CDO
        ///     memory and returns a reference to the new record.
        /// </summary>
        R Create(R record);

        /// <summary>
        ///     Initializes CDO memory with record objects from the data
        ///     records in a single table, or in one or more tables of a
        ///     ProDataSet, as returned by the built-in read operation of the
        ///     resource for which the CDO is created.
        /// </summary>
        Task<ICDORequest> Fill(Expression<Func<R, bool>> filter);

        /// <summary>
        ///     Searches for a record in a table referenced in CDO memory
        ///     and returns a reference to that record if found. If no record
        ///     is found, it returns null.
        /// </summary>
        /// <returns></returns>
        Task<R> Find(Expression<Func<R, bool>> filter, bool autoFetch);

        /// <summary>
        ///     Searches for a record in a table referenced in CDO memory
        ///     and returns a reference to a dataset with all related data from the record if found. If no record
        ///     is found, it returns null.
        /// </summary>
        /// <returns></returns>
        Task<D> Get(Expression<Func<R, bool>> filter);

        /// <summary>
        ///     Locates and returns the record in CDO memory with the
        ///     internal ID you specify.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<R> FindById(string id, bool autoFetch);

        /// <summary>
        ///     Returns an array of record objects for a table referenced in
        ///     CDO memory
        /// </summary>
        /// <returns></returns>
        R[] GetData();
        /// <summary>
        ///     Initializes CDO memory with record objects from the data
        ///     records in a single table, or in one or more tables of a
        ///     ProDataSet, as returned by the built-in read operation of the
        ///     resource for which the CDO is created.
        /// </summary>
        Task<ICDORequest> Read(Expression<Func<R, bool>> filter);
        /// <summary>
        ///     Deletes the specified table record referenced in CDO
        ///     memory.
        /// </summary>
        bool Remove(R record);

        /// <summary>
        /// Reset CDO Memory
        /// </summary>
        void Reset();
        #endregion

        #region Properties

        /// <summary>
        ///     An object reference property on a CDO that has the name
        ///     of a table mapped by the resource to a table for which the
        ///     current CDO is created.
        ///     This can be multiple tables
        ///     In case of a gneric CDO this is a reference to the CDO_Dataset
        ///     The CDO_CLientgen generates typed ref to tables
        /// </summary>
        CDO_Table<R> TableReference { get; }

        /// <summary>
        ///     A property on a CDO table reference that references a
        ///     CloudDataRecord object with the data for the
        ///     working record of a table referenced in CDO memory.
        /// </summary>
        R Record { get; set; }

        #endregion

        #region Events

        /// <summary>
        ///     Fires after the CDO, by means of a saveChanges( ) call
        ///     following an add() or create() call, sends a request to
        ///     create a record and receives a response to this request
        ///     from the Cloud Data Server.
        /// </summary>
        event EventHandler<CDOEventArgs<T, D, R>> AfterCreate;

        /// <summary>
        ///     Fires after the CDO, by means of a saveChanges( ) call
        ///     following a remove() call, sends a request to delete a
        ///     record and receives a response to this request from the
        ///     Cloud Data Server.
        /// </summary>
        event EventHandler<CDOEventArgs<T, D, R>> AfterDelete;

        /// <summary>
        ///     Fires after the CDO, by means of a fill( ) call, sends a
        ///     request to read a table or ProDataSet into CDO memory
        ///     and receives a response to this request from the Cloud Data
        ///     Server.
        /// </summary>
        event EventHandler<CDOEventArgs<T, D, R>> AfterFill;

        /// <summary>
        ///     Fires after a non-built-in method is called asynchronously
        ///     on a CDO and a response to the request is received from
        ///     the Cloud Data Server.
        /// </summary>
        event EventHandler<CDOEventArgs<T, D, R>> AfterInvoke;

        /// <summary>
        ///     Fires after the CDO, by means of a read( ) call, sends a
        ///     request to read a table or ProDataSet into CDO memory
        ///     and receives a response to this request from the Cloud Data
        ///     Server.
        /// </summary>
        event EventHandler<CDOEventArgs<T, D, R>> AfterRead;

        /// <summary>
        ///     Fires once for each call to the saveChanges( ) method
        ///     on a CDO, after responses to all create, update, and delete
        ///     requests have been received from the Cloud Data Server.
        /// </summary>
        event EventHandler<CDOEventArgs<T, D, R>> AfterSaveChanges;

        /// <summary>
        ///     Fires after the CDO, by means of a saveChanges( ) call
        ///     following an assign( ) or update() call, sends a request
        ///     to update a record and receives a response to this
        ///     request from the Cloud Data Server.
        /// </summary>
        event EventHandler<CDOEventArgs<T, D, R>> AfterUpdate;

        /// <summary>
        ///     Fires before the CDO, by means of a saveChanges( )
        ///     call making an add( ) or create() call, sends a request the
        ///     Cloud Data Server to create a record.
        /// </summary>
        event EventHandler<CDOEventArgs<T, D, R>> BeforeCreate;

        /// <summary>
        ///     Fires before the CDO, by means of a saveChanges( )
        ///     call making a remove( ) call, sends a request the
        ///     Cloud Data Server to delete a record.
        /// </summary>
        event EventHandler<CDOEventArgs<T, D, R>> BeforeDelete;

        /// <summary>
        ///     Fires before the CDO, by means of a fill( ) call, sends
        ///     a request to the Cloud Data Server to read a table or
        ///     ProDataSet into CDO memory.
        /// </summary>
        event EventHandler<CDOEventArgs<T, D, R>> BeforeFill;

        /// <summary>
        ///     Fires when a non-built-in method is called asynchronously
        ///     on a CDO, before the request for the operation is sent to
        ///     the Cloud Data Server.
        /// </summary>
        event EventHandler<CDOEventArgs<T, D, R>> BeforeInvoke;

        /// <summary>
        ///     Fires before the CDO, by means of a read( ) call, sends
        ///     a request to the Cloud Data Server to read a table or
        ///     ProDataSet into CDO memory.
        /// </summary>
        event EventHandler<CDOEventArgs<T, D, R>> BeforeRead;

        /// <summary>
        ///     Fires once for each call to the saveChanges( ) method
        ///     on a CDO, before any create, update, or delete requests
        ///     are sent to the Cloud Data Server.
        /// </summary>
        event EventHandler<CDOEventArgs<T, D, R>> BeforeSaveChanges;

        /// <summary>
        ///     Fires before the CDO, by means of a saveChanges( )
        ///     call making an assign( ) or update() call, sends a
        ///     request the Cloud Data Server to update a record.
        /// </summary>
        event EventHandler<CDOEventArgs<T, D, R>> BeforeUpdate;

        #endregion
    }

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
            return await Fill(new QueryRequest() { ABLFilter = filter.ToABLFIlter() });
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
                var primaryKey = (string)pkFieldInfo.GetValue(defaultsFieldInfo.GetValue(null));
                if (string.IsNullOrEmpty(primaryKey))
                {
                    new R();
                    primaryKey = (string)pkFieldInfo.GetValue(defaultsFieldInfo.GetValue(null));
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
                ObjParam = _serviceDefinition.UseRequest ? new JsonObject { { "request", inputObject } } : inputObject,
                RequestUri =
                    new Uri(
                        $"{_cDOSession.ServiceURI.AbsoluteUri}{_serviceDefinition.Address}{_resourceDefinition.Path}{operationDefinition.Path}",
                        UriKind.Absolute),
                Method = new HttpMethod(operationDefinition.Verb.ToString().ToUpper())
            };

            BeforeInvoke?.Invoke(this,
                new CDOEventArgs<T, D, R> { CDO = this, Request = cDORequest, Session = _cDOSession });
            await DoRequest(cDORequest, ProcessInvokeResponse);
            AfterInvoke?.Invoke(this, new CDOEventArgs<T, D, R> { CDO = this, Request = cDORequest, Session = _cDOSession });

            return cDORequest;
        }

        public async Task<ICDORequest> Read(QueryRequest queryRequest = null)
        {
            return await Read(queryRequest?.ToString(_resourceDefinition.Operations.FirstOrDefault(o => o.Type == OperationType.Read)?.Capabilities));
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

            BeforeFill?.Invoke(this, new CDOEventArgs<T, D, R> { CDO = this, Request = cDORequest, Session = _cDOSession });
            BeforeRead?.Invoke(this, new CDOEventArgs<T, D, R> { CDO = this, Request = cDORequest, Session = _cDOSession });
            await DoRequest(cDORequest, ProcessCRUDResponse);
            AfterFill?.Invoke(this, new CDOEventArgs<T, D, R> { CDO = this, Request = cDORequest, Session = _cDOSession });
            AfterRead?.Invoke(this, new CDOEventArgs<T, D, R> { CDO = this, Request = cDORequest, Session = _cDOSession });

            return cDORequest;
        }

        public async Task<ICDORequest> Read(Expression<Func<R, bool>> filter)
        {
            return await Read(new QueryRequest() { ABLFilter = filter.ToABLFIlter() });
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

        public async Task SaveChanges()
        {
            BeforeSaveChanges?.Invoke(this, new CDOEventArgs<T, D, R> { CDO = this, Request = null });

            if (TableReference != null)
            {
                Operation operation = null;
                //delete
                if (TableReference._deleted.Any())
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
                        ObjParam = new D { { _mainTable, new CDO_Table<R>(TableReference._deleted) } }
                    };
                    BeforeDelete?.Invoke(this, new CDOEventArgs<T, D, R> { CDO = this, Request = deleteRequest, Session = _cDOSession });
                    await DoRequest(deleteRequest, ProcessCRUDResponse);
                    AfterDelete?.Invoke(this, new CDOEventArgs<T, D, R> { CDO = this, Request = deleteRequest, Session = _cDOSession });
                }

                //create
                if (TableReference._new.Any())
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
                        ObjParam = new D { { _mainTable, new CDO_Table<R>(TableReference._new) } }
                    };
                    BeforeCreate?.Invoke(this, new CDOEventArgs<T, D, R> { CDO = this, Request = createRequest, Session = _cDOSession });
                    await DoRequest(createRequest, ProcessCRUDResponse);
                    AfterCreate?.Invoke(this, new CDOEventArgs<T, D, R> { CDO = this, Request = createRequest, Session = _cDOSession });
                }
                //update
                if (TableReference._changed.Any())
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
                        ObjParam = new D { { _mainTable, new CDO_Table<R>(TableReference._changed) } }
                    };
                    BeforeUpdate?.Invoke(this, new CDOEventArgs<T, D, R> { CDO = this, Request = updateRequest, Session = _cDOSession });
                    await DoRequest(updateRequest, ProcessCRUDResponse);
                    AfterUpdate?.Invoke(this, new CDOEventArgs<T, D, R> { CDO = this, Request = updateRequest, Session = _cDOSession });
                }

                //all done => accept the changes
                if (AutoApplyChanges) TableReference.AcceptChanges();
            }
            else throw new CDOException(string.Format(Properties.Resources.API_InternalError, "SaveChanges"));
            AfterSaveChanges?.Invoke(this, new CDOEventArgs<T, D, R> { CDO = this, Request = null });
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
                    request.Content = new StringContent(cDORequest.ObjParam.ToString(), Encoding.UTF8, "application/json");
                using (var response = await _cDOSession.HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, default(CancellationToken)))
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
            if (response.IsSuccessStatusCode && response.Content.Headers.ContentLength > 0)
                using (var dataStream = await response.Content.ReadAsStreamAsync())
                {
                    if (dataStream != null)
                        request.Response = (JsonObject)(string.IsNullOrEmpty(request.FnName)
                            ? JsonValue.Load(dataStream)
                            : JsonValue.Load(dataStream).Get("response"));
                }
        }

        public virtual async Task ProcessCRUDResponse(HttpResponseMessage response, CDORequest request)
        {
            await ProcessInvokeResponse(response, request);
            if (request.Success.HasValue && request.Success.Value)
                if (request.Method == HttpMethod.Get || request.Method == HttpMethod.Post || request.Method == HttpMethod.Put)
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
            _primaryKey = _resourceDefinition.Schema?.Properties.FirstOrDefault().Value.Properties[_mainTable].PrimaryKey.FirstOrDefault();
        }

        #endregion
    }
}