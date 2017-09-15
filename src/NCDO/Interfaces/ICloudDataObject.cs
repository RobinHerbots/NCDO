using System;
using NCDO.Definitions;
using System.Linq;
using System.Data;
using NCDO.Catalog;
using System.Json;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;

namespace NCDO.Interfaces
{
    /// <summary>
    /// The CDO provides access to resources in a Cloud Data Service, known as a Cloud Data Resource. 
    /// </summary>
    public interface ICloudDataObject
    {
        #region Properties
        /// <summary>
        /// A Boolean on a CDO that indicates if the CDO
        ///automatically accepts or rejects changes to CDO memory
        ///when you call the saveChanges( ) method.
        /// </summary>
        bool AutoApplyChanges { get; set; }
        /// <summary>
        /// A Boolean on a CDO and its table references that
        ///indicates if record objects are sorted automatically on the
        ///affected table references in CDO memory at the completion
        ///of a supported CDO operation.
        /// </summary>
        bool AutoSort { get; set; }
        /// <summary>
        /// A Boolean on a CDO and its table references that
        ///indicates if String field comparisons performed by
        ///supported CDO operations are case sensitive or
        ///case-insensitive for the affected table references in CDO
        ///memory
        /// </summary>
        bool CaseSensitive { get; set; }
        /// <summary>
        /// The name of the resource for which the current CDO is
        ///created.
        /// </summary>
        string Name { get; }
        /// <summary>
        /// An object reference property on a CDO that has the name
        ///of a table mapped by the resource to a table for which the
        ///current CDO is created.
        ///This can be multiple tables
        /// </summary>
        JsonObject TableReference { get; }
        /// <summary>
        /// A Boolean that specifies whether CDO methods that
        ///operate on table references in CDO memory work with the
        ///table relationships defined in the schema (that is, work only
        ///on the records of a child table that are related to the parent).
        /// </summary>
        bool UseRelationships { get; set; }
        #endregion
        #region Methods
        /// <summary>
        /// Accepts changes to the data in CDO memory for the
        ///specified table reference or for all table references of the
        ///specified CDO.
        /// </summary>
        void AcceptChanges();
        /// <summary>
        /// Accepts changes to the data in CDO memory for a specified
        ///record object.
        /// </summary>
        bool AcceptRowChanges();
        /// <summary>
        /// Reads the record objects stored in the specified local storage
        ///area and updates CDO memory based on these record
        ///objects, including any pending changes and before-image
        ///data, if they exist.
        /// </summary>
        void AddLocalRecords();

        /// <summary>
        /// Reads an array, table, or ProDataSet object containing one
        ///or more record objects and updates CDO memory based
        ///on these record objects, including any pending changes and
        ///before-image data, if they exist.
        /// </summary>
        void AddRecords(MergeMode mergeMode = MergeMode.Replace);
        /// <summary>
        /// Updates field values for the specified CloudDataRecord
        ///object in CDO memory.
        /// </summary>
        void Assign();
        /// <summary>
        /// Clears out all data and changes stored in a specified local
        ///storage area, and removes the cleared storage area.
        /// </summary>
        void DeleteLocal();
        /// <summary>
        /// Initializes CDO memory with record objects from the data
        ///records in a single table, or in one or more tables of a
        ///ProDataSet, as returned by the built-in read operation of the
        ///resource for which the CDO is created.
        /// </summary>
        Task<ICDORequest> Fill(QueryRequest queryRequest = null);
        /// <summary>
        /// Returns an array of errors from the most recent CDO
        ///operation.
        /// </summary>
        /// <returns></returns>
        string[] GetErrors();
        /// <summary>
        /// Returns any before-image error string in the data of a record
        ///object referenced in CDO memory that was set as the result
        ///of a Data Object create, update, delete, or submit
        ///operation.
        /// </summary>
        /// <returns></returns>
        string GetErrorString();
        /// <summary>
        /// Returns the unique internal ID for the record object
        ///referenced in CDO memory.
        /// </summary>
        /// <returns></returns>
        string GetId();
        /// <summary>
        /// Returns an array of objects, one for each field that defines
        ///the schema of a table referenced in CDO memory.
        /// </summary>
        void GetSchema();
        /// <summary>
        /// Returns true if record objects can be found in any of the
        ///tables referenced in CDO memory (with or without pending
        ///changes), or in only the single table referenced on the CDO,
        ///depending on how the method is called; and returns false
        ///if no record objects are found in either case.
        /// </summary>
        /// <returns></returns>
        bool HasData();
        /// <summary>
        /// Returns true if CDO memory contains any pending
        ///changes (with or without before-image data), and returns
        ///false if CDO memory has no pending changes.
        /// </summary>
        /// <returns></returns>
        bool HasChanges();
        /// <summary>
        /// Asynchronously calls a custom invocation method on the
        ///CDO to execute an Invoke operation defined by a Data
        ///Object resource.
        /// </summary>
        Task<ICDORequest> Invoke(string operation, JsonObject inputObject = null);
        /// <summary>
        /// Initializes CDO memory with record objects from the data
        ///records in a single table, or in one or more tables of a
        ///ProDataSet, as returned by the built-in read operation of the
        ///resource for which the CDO is created.
        /// </summary>
        Task<ICDORequest> Read(QueryRequest queryRequest = null);
        /// <summary>
        /// Clears out the data in CDO memory and replaces it with all
        ///the data stored in a specified local storage area, including
        ///any pending changes and before-image data, if they exist.
        /// </summary>
        void ReadLocal();
        /// <summary>
        /// Rejects changes to the data in CDO memory for the
        ///specified table reference or for all table references of the
        ///specified CDO.
        /// </summary>
        void RejectChanges();
        /// <summary>
        /// Rejects changes to the data in CDO memory for a specified
        ///record object.
        /// </summary>
        void RejectRowChanges();
        /// <summary>
        /// Deletes the specified table record referenced in CDO
        ///memory.
        /// </summary>
        void Remove();
        /// <summary>
        /// Synchronizes to the Cloud Data Server all changes pending in
        ///CDO memory since the last call to the fill( ) or
        ///saveChanges( ) methods, or since any prior changes
        ///have been otherwise accepted or rejected.
        /// </summary>
        void SaveChanges();
        /// <summary>
        /// Saves CDO memory to a specified local storage area,
        ///including pending changes and any before-image data,
        ///according to a specified data mode.
        /// </summary>
        void SaveLocal();
        /// <summary>
        /// Specifies or clears the record fields on which to automatically
        ///sort the record objects for a table reference after you have
        ///set its autoSort property to true (the default).
        /// </summary>
        void SetSortFields();
        /// <summary>
        /// Specifies or clears a user-defined sort function with which
        ///to automatically sort the record objects for a table reference
        ///after you have set its autoSort property to true (the
        ///default).
        /// </summary>
        void SetSortFn();
        /// <summary>
        /// Sorts the existing record objects for a table reference in
        ///CDO memory using either specified sort fields or a specified
        ///user-defined sort function.
        /// </summary>
        void Sort();

        #endregion
    }

    /// <summary>
    /// The CDO provides access to resources in a Cloud Data Service, known as a Cloud Data Resource. 
    /// </summary>
    public interface ICloudDataObject<T> : ICloudDataObject where T : class
    {


        #region Properties
        /// <summary>
        /// A property on a CDO table reference that references a
        ///CloudDataRecord object with the data for the
        ///working record of a table referenced in CDO memory.
        /// </summary>
        ICloudDataRecord<T> Record { get; set; }
        #endregion
        #region Methods
        /// <summary>
        /// Creates a new record object for a table referenced in CDO
        ///memory and returns a reference to the new record.
        /// </summary>
        ICloudDataRecord<T> Add();
        /// <summary>
        /// Creates a new record object for a table referenced in CDO
        ///memory and returns a reference to the new record.
        /// </summary>
        ICloudDataRecord<T> Create();
        /// <summary>
        /// Searches for a record in a table referenced in CDO memory
        ///and returns a reference to that record if found. If no record
        ///is found, it returns null.
        /// </summary>
        /// <returns></returns>
        ICloudDataRecord<T> Find();
        /// <summary>
        /// Locates and returns the record in CDO memory with the
        ///internal ID you specify.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        ICloudDataRecord<T> FindById(string id);
        /// <summary>
        /// Returns an array of record objects for a table referenced in
        ///CDO memory
        /// </summary>
        /// <returns></returns>
        ICloudDataRecord<T>[] GetData();

        Task ProcessCRUDResponse(HttpClient client, HttpResponseMessage response, CDORequest request);
        Task ProcessInvokeResponse(HttpClient client, HttpResponseMessage response, CDORequest request);
        #endregion
        #region Events
        /// <summary>
        /// Fires after the CDO, by means of a saveChanges( ) call
        ///following an add() or create() call, sends a request to
        ///create a record and receives a response to this request
        ///from the Cloud Data Server.
        /// </summary>
        event EventHandler<CDOEventArgs<T>> AfterCreate;
        /// <summary>
        /// Fires after the CDO, by means of a saveChanges( ) call
        ///following a remove() call, sends a request to delete a
        ///record and receives a response to this request from the
        ///Cloud Data Server.
        /// </summary>
        event EventHandler<CDOEventArgs<T>> AfterDelete;
        /// <summary>
        /// Fires after the CDO, by means of a fill( ) call, sends a
        ///request to read a table or ProDataSet into CDO memory
        ///and receives a response to this request from the Cloud Data
        ///Server.
        /// </summary>
        event EventHandler<CDOEventArgs<T>> AfterFill;
        /// <summary>
        /// Fires after a non-built-in method is called asynchronously
        ///on a CDO and a response to the request is received from
        ///the Cloud Data Server.
        /// </summary>
        event EventHandler<CDOEventArgs<T>> AfterInvoke;
        /// <summary>
        /// Fires after the CDO, by means of a read( ) call, sends a
        ///request to read a table or ProDataSet into CDO memory
        ///and receives a response to this request from the Cloud Data
        ///Server.
        /// </summary>
        event EventHandler<CDOEventArgs<T>> AfterRead;
        /// <summary>
        /// Fires once for each call to the saveChanges( ) method
        ///on a CDO, after responses to all create, update, and delete
        ///requests have been received from the Cloud Data Server.
        /// </summary>
        event EventHandler<CDOEventArgs<T>> AfterSaveChanges;
        /// <summary>
        /// Fires after the CDO, by means of a saveChanges( ) call
        ///following an assign( ) or update() call, sends a request
        ///to update a record and receives a response to this
        ///request from the Cloud Data Server.
        /// </summary>
        event EventHandler<CDOEventArgs<T>> AfterUpdate;
        /// <summary>
        /// Fires before the CDO, by means of a saveChanges( )
        ///call making an add( ) or create() call, sends a request the
        ///Cloud Data Server to create a record.
        /// </summary>
        event EventHandler<CDOEventArgs<T>> BeforeCreate;
        /// <summary>
        /// Fires before the CDO, by means of a saveChanges( )
        ///call making a remove( ) call, sends a request the
        ///Cloud Data Server to delete a record.
        /// </summary>
        event EventHandler<CDOEventArgs<T>> BeforeDelete;
        /// <summary>
        /// Fires before the CDO, by means of a fill( ) call, sends
        ///a request to the Cloud Data Server to read a table or
        ///ProDataSet into CDO memory.
        /// </summary>
        event EventHandler<CDOEventArgs<T>> BeforeFill;
        /// <summary>
        /// Fires when a non-built-in method is called asynchronously
        ///on a CDO, before the request for the operation is sent to
        ///the Cloud Data Server.
        /// </summary>
        event EventHandler<CDOEventArgs<T>> BeforeInvoke;
        /// <summary>
        /// Fires before the CDO, by means of a read( ) call, sends
        ///a request to the Cloud Data Server to read a table or
        ///ProDataSet into CDO memory.
        /// </summary>
        event EventHandler<CDOEventArgs<T>> BeforeRead;
        /// <summary>
        /// Fires once for each call to the saveChanges( ) method
        ///on a CDO, before any create, update, or delete requests
        ///are sent to the Cloud Data Server.
        /// </summary>
        event EventHandler<CDOEventArgs<T>> BeforeSaveChanges;
        /// <summary>
        /// Fires before the CDO, by means of a saveChanges( )
        ///call making an assign( ) or update() call, sends a
        ///request the Cloud Data Server to update a record.
        /// </summary>
        event EventHandler<CDOEventArgs<T>> BeforeUpdate;

        #endregion
    }

    public abstract class ACloudDataObject<T> : ICloudDataObject<T>
        where T : class
    {
        private ICDOSession _cDOSession;
        private CDOMemory _cdoMemory;
        private Service _serviceDefinition;
        private Resource _resourceDefinition;

        /// <summary>
        /// CDO Constructor
        /// </summary>
        /// <param name="resource">A string expression set to the name of a resource provided by a Cloud Data Service for
        /// which a login session has been started.
        ///</param>
        ///<param name="autoFill">
        ///Specifies whether the CDO invokes its fill( ) method upon instantiation to initialize CDO memory with data from the resource.</param>
        protected ACloudDataObject(string resource, ICDOSession cDOSession = null, bool autoFill = false)
        {
            _cDOSession = cDOSession;
            if (_cDOSession == null)
            {
                //pickup last initialized session
                _cDOSession = CDOSession.Instance;
                if (_cDOSession == null)
                {
                    throw new Exception("No session available");
                }
            }
            Name = resource;

            InitCDO(autoFill);
        }



        public bool AutoApplyChanges { get; set; }
        public bool AutoSort { get; set; }
        public bool CaseSensitive { get; set; }
        public string Name { get; }
        public JsonObject TableReference { get; }
        public bool UseRelationships { get; set; }
        public void AcceptChanges()
        {
        }

        public bool AcceptRowChanges()
        {

            return true;
        }

        public void AddLocalRecords()
        {
            throw new NotImplementedException();
        }

        public void AddRecords(MergeMode mergeMode = MergeMode.Replace)
        {
            throw new NotImplementedException();
        }

        public void Assign()
        {
            throw new NotImplementedException();
        }

        public void DeleteLocal()
        {
            throw new NotImplementedException();
        }

        public async Task<ICDORequest> Fill(QueryRequest queryRequest = null)
        {
            return await Read(queryRequest);
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

        public void GetSchema()
        {
            throw new NotImplementedException();
        }

        public bool HasData()
        {
            throw new NotImplementedException();
        }

        public bool HasChanges()
        {
            throw new NotImplementedException();
        }

        public async Task<ICDORequest> Invoke(string operation, JsonObject inputObject = null)
        {
            var operationDefinition = VerifyOperation(operation);
            //init request if needed
            var cDORequest = new CDORequest
            {
                CDO = this,
                FnName = operationDefinition.Name,
                ObjParam = _serviceDefinition.UseRequest ? new JsonObject() { { "request", inputObject } } : inputObject,
                RequestUri = new Uri($"{_cDOSession.ServiceURI.AbsoluteUri}{_serviceDefinition.Address}{_resourceDefinition.Path}{operationDefinition.Path}", UriKind.Absolute),
                Method = new HttpMethod(operationDefinition.Verb.ToString().ToUpper())
            };

            BeforeInvoke?.Invoke(this, new CDOEventArgs<T> { CDO = this, Request = cDORequest, Session = _cDOSession });
            await DoRequest(cDORequest, ProcessInvokeResponse);
            AfterInvoke?.Invoke(this, new CDOEventArgs<T> { CDO = this, Request = cDORequest, Session = _cDOSession });

            return cDORequest;
        }



        public async Task<ICDORequest> Read(QueryRequest queryRequest = null)
        {
            var operationDefinition = VerifyOperation(null, OperationType.Read);

            //setup filter for get request
            var filterpath = queryRequest == null ? "" : operationDefinition.Path.ToString().Replace("{filter}", queryRequest.ToString());

            //init request if needed
            var cDORequest = new CDORequest
            {
                CDO = this,
                FnName = operationDefinition.Name,
                RequestUri = new Uri($"{_cDOSession.ServiceURI.AbsoluteUri}{_serviceDefinition.Address}{_resourceDefinition.Path}{filterpath}", UriKind.Absolute),
                Method = new HttpMethod(operationDefinition.Verb.ToString().ToUpper())
            };

            BeforeFill?.Invoke(this, new CDOEventArgs<T> { CDO = this, Request = cDORequest, Session = _cDOSession });
            BeforeRead?.Invoke(this, new CDOEventArgs<T> { CDO = this, Request = cDORequest, Session = _cDOSession });
            await DoRequest(cDORequest, ProcessCRUDResponse);
            AfterFill?.Invoke(this, new CDOEventArgs<T> { CDO = this, Request = cDORequest, Session = _cDOSession });
            AfterRead?.Invoke(this, new CDOEventArgs<T> { CDO = this, Request = cDORequest, Session = _cDOSession });

            return cDORequest;
        }

        public void ReadLocal()
        {
            throw new NotImplementedException();
        }

        public void RejectChanges()
        {
            throw new NotImplementedException();
        }

        public void RejectRowChanges()
        {
            throw new NotImplementedException();
        }

        public void Remove()
        {
            throw new NotImplementedException();
        }

        public void SaveChanges()
        {
            BeforeSaveChanges?.Invoke(this, new CDOEventArgs<T> { CDO = this, Request = null });
            throw new NotImplementedException();
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



        public ICloudDataRecord<T> Record { get; set; }
        public ICloudDataRecord<T> Add()
        {
            return Create();
        }

        public ICloudDataRecord<T> Create()
        {
            BeforeCreate?.Invoke(this, new CDOEventArgs<T>() { CDO = this, Session = _cDOSession, Request = null, Record = null });


            AfterCreate?.Invoke(this, new CDOEventArgs<T>() { CDO = this, Session = _cDOSession });

            return null;
        }

        public ICloudDataRecord<T> Find()
        {
            throw new NotImplementedException();
        }

        public ICloudDataRecord<T> FindById(string id)
        {
            throw new NotImplementedException();
        }

        public ICloudDataRecord<T>[] GetData()
        {
            throw new NotImplementedException();
        }
        #region Events
        public event EventHandler<CDOEventArgs<T>> AfterCreate;
        public event EventHandler<CDOEventArgs<T>> AfterDelete;
        public event EventHandler<CDOEventArgs<T>> AfterFill;
        public event EventHandler<CDOEventArgs<T>> AfterInvoke;
        public event EventHandler<CDOEventArgs<T>> AfterRead;
        public event EventHandler<CDOEventArgs<T>> AfterSaveChanges;
        public event EventHandler<CDOEventArgs<T>> AfterUpdate;
        public event EventHandler<CDOEventArgs<T>> BeforeCreate;
        public event EventHandler<CDOEventArgs<T>> BeforeDelete;
        public event EventHandler<CDOEventArgs<T>> BeforeFill;
        public event EventHandler<CDOEventArgs<T>> BeforeInvoke;
        public event EventHandler<CDOEventArgs<T>> BeforeRead;
        public event EventHandler<CDOEventArgs<T>> BeforeSaveChanges;
        public event EventHandler<CDOEventArgs<T>> BeforeUpdate;
        #endregion
        #region Request logic
        public virtual async Task<ICDORequest> DoRequest(CDORequest cDORequest, Func<HttpClient, HttpResponseMessage, CDORequest, Task> processResponse)
        {
            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage())
                {
                    //add authentication headers
                    _cDOSession.OnOpenRequest(client, request);

                    //init request from CDORequest
                    request.Method = cDORequest.Method;
                    request.RequestUri = cDORequest.RequestUri;
                    if (!cDORequest.Method.Equals(HttpMethod.Get))
                        request.Content = new StringContent(cDORequest.ObjParam.ToString(), Encoding.UTF8, "application/json");

                    var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead);
                    await processResponse?.Invoke(client, response, cDORequest);
                }
            }
            return cDORequest;
        }
        public virtual async Task ProcessInvokeResponse(HttpClient client, HttpResponseMessage response, CDORequest request)
        {
            request.Success = response.IsSuccessStatusCode;
            request.ResponseMessage = response;
            if (response.IsSuccessStatusCode)
            {
                using (var dataStream = await response.Content.ReadAsStreamAsync())
                {
                    if (dataStream != null)
                    {
                        request.Response = (JsonObject)JsonObject.Load(dataStream);
                    }
                }
            }
        }
        public virtual async Task ProcessCRUDResponse(HttpClient client, HttpResponseMessage response, CDORequest request)
        {
            await ProcessInvokeResponse(client, response, request);
            if (request.Success.HasValue && request.Success.Value)
            {
                if (request.Method == HttpMethod.Get)
                {
                    //init cdoMemory
                    _cdoMemory = new CDOMemory(request.Response);
                }
                else
                {
                    //TODO merge changes to cdoMemory or notitfy cdomemory 
                }
            }
        }
        #endregion
        #region private 
        private void InitCDO(bool autoFill)
        {
            VerifyResourceName(Name);

            if (autoFill)
            {
                Fill().Wait();
                Console.WriteLine(_cdoMemory.ToString());
            }
        }
        /// <summary>
        /// Verify if the resource is available and return the catalog definition for the catalog
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        private Resource VerifyResourceName(string resource)
        {
            _serviceDefinition = _cDOSession.Services.FirstOrDefault(s => s.Resources.Any(r => r.Name.Equals(resource)));
            var resourceDefinition = _serviceDefinition?.Resources.FirstOrDefault(r => r.Name.Equals(resource));
            if (resourceDefinition == null)
            {
                throw new NotSupportedException($"Invalid resourcename {resource}.");
            }
            return _resourceDefinition = resourceDefinition;
        }
        private Operation VerifyOperation(string operation, OperationType operationType = OperationType.Invoke)
        {
            var operationDefinition = _resourceDefinition.Operations.FirstOrDefault(o => o.Type == operationType && (string.IsNullOrEmpty(operation) || o.Name.Equals(operation)));
            if (operationDefinition == null)
            {
                throw new NotSupportedException($"Invalid {operationType} operation {operation}.");
            }
            return operationDefinition;
        }




        #endregion
    }
}
