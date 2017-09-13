using System;
using NCDO.Definitions;
using System.Linq;
using System.Data;

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
        DataTableCollection TableReference { get; }
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
        void AcceptRowChanges();
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
        void fill();
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
        void Invoke();
        /// <summary>
        /// Initializes CDO memory with record objects from the data
        ///records in a single table, or in one or more tables of a
        ///ProDataSet, as returned by the built-in read operation of the
        ///resource for which the CDO is created.
        /// </summary>
        void Read();
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
        #region Events
        /// <summary>
        /// Fires after the CDO, by means of a saveChanges( ) call
        ///following an add() or create() call, sends a request to
        ///create a record and receives a response to this request
        ///from the Cloud Data Server.
        /// </summary>
        event EventHandler<CDOEventArgs> AfterCreate;
        /// <summary>
        /// Fires after the CDO, by means of a saveChanges( ) call
        ///following a remove() call, sends a request to delete a
        ///record and receives a response to this request from the
        ///Cloud Data Server.
        /// </summary>
        event EventHandler<CDOEventArgs> AfterDelete;
        /// <summary>
        /// Fires after the CDO, by means of a fill( ) call, sends a
        ///request to read a table or ProDataSet into CDO memory
        ///and receives a response to this request from the Cloud Data
        ///Server.
        /// </summary>
        event EventHandler<CDOEventArgs> AfterFill;
        /// <summary>
        /// Fires after a non-built-in method is called asynchronously
        ///on a CDO and a response to the request is received from
        ///the Cloud Data Server.
        /// </summary>
        event EventHandler<CDOEventArgs> AfterInvoke;
        /// <summary>
        /// Fires after the CDO, by means of a read( ) call, sends a
        ///request to read a table or ProDataSet into CDO memory
        ///and receives a response to this request from the Cloud Data
        ///Server.
        /// </summary>
        event EventHandler<CDOEventArgs> AfterRead;
        /// <summary>
        /// Fires once for each call to the saveChanges( ) method
        ///on a CDO, after responses to all create, update, and delete
        ///requests have been received from the Cloud Data Server.
        /// </summary>
        event EventHandler<CDOEventArgs> AfterSaveChanges;
        /// <summary>
        /// Fires after the CDO, by means of a saveChanges( ) call
        ///following an assign( ) or update() call, sends a request
        ///to update a record and receives a response to this
        ///request from the Cloud Data Server.
        /// </summary>
        event EventHandler<CDOEventArgs> AfterUpdate;
        /// <summary>
        /// Fires before the CDO, by means of a saveChanges( )
        ///call making an add( ) or create() call, sends a request the
        ///Cloud Data Server to create a record.
        /// </summary>
        event EventHandler<CDOEventArgs> BeforeCreate;
        /// <summary>
        /// Fires before the CDO, by means of a saveChanges( )
        ///call making a remove( ) call, sends a request the
        ///Cloud Data Server to delete a record.
        /// </summary>
        event EventHandler<CDOEventArgs> BeforeDelete;
        /// <summary>
        /// Fires before the CDO, by means of a fill( ) call, sends
        ///a request to the Cloud Data Server to read a table or
        ///ProDataSet into CDO memory.
        /// </summary>
        event EventHandler<CDOEventArgs> BeforeFill;
        /// <summary>
        /// Fires when a non-built-in method is called asynchronously
        ///on a CDO, before the request for the operation is sent to
        ///the Cloud Data Server.
        /// </summary>
        event EventHandler<CDOEventArgs> BeforeInvoke;
        /// <summary>
        /// Fires before the CDO, by means of a read( ) call, sends
        ///a request to the Cloud Data Server to read a table or
        ///ProDataSet into CDO memory.
        /// </summary>
        event EventHandler<CDOEventArgs> BeforeRead;
        /// <summary>
        /// Fires once for each call to the saveChanges( ) method
        ///on a CDO, before any create, update, or delete requests
        ///are sent to the Cloud Data Server.
        /// </summary>
        event EventHandler<CDOEventArgs> BeforeSaveChanges;
        /// <summary>
        /// Fires before the CDO, by means of a saveChanges( )
        ///call making an assign( ) or update() call, sends a
        ///request the Cloud Data Server to update a record.
        /// </summary>
        event EventHandler<CDOEventArgs> BeforeUpdate;

        #endregion
    }

    /// <summary>
    /// The CDO provides access to resources in a Cloud Data Service, known as a Cloud Data Resource. 
    /// </summary>
    interface ICloudDataObject<T> : ICloudDataObject where T : class
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
        #endregion
    }

    public abstract class ACloudDataObject<T> : ICloudDataObject<T>
        where T : class
    {
        private ICDOSession _cDOSession;
        private DataSet _cdoMemory = new DataSet();

        /// <summary>
        /// CDO Constructor
        /// </summary>
        /// <param name="respource">A string expression set to the name of a resource provided by a Cloud Data Service for
        /// which a login session has been started.
        ///</param>
        protected ACloudDataObject(string respource, ICDOSession cDOSession = null)
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
            VerifyResourceName(respource);
            Name = respource;
        }

        public bool AutoApplyChanges { get; set; }
        public bool AutoSort { get; set; }
        public bool CaseSensitive { get; set; }
        public string Name { get; }
        public DataTableCollection TableReference => _cdoMemory.Tables;
        public bool UseRelationships { get; set; }
        public void AcceptChanges()
        {
            throw new NotImplementedException();
        }

        public void AcceptRowChanges()
        {
            throw new NotImplementedException();
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

        public void fill()
        {
            throw new NotImplementedException();
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

        public void Invoke()
        {
            throw new NotImplementedException();
        }

        public void Read()
        {
            throw new NotImplementedException();
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

        public event EventHandler<CDOEventArgs> AfterCreate;
        public event EventHandler<CDOEventArgs> AfterDelete;
        public event EventHandler<CDOEventArgs> AfterFill;
        public event EventHandler<CDOEventArgs> AfterInvoke;
        public event EventHandler<CDOEventArgs> AfterRead;
        public event EventHandler<CDOEventArgs> AfterSaveChanges;
        public event EventHandler<CDOEventArgs> AfterUpdate;
        public event EventHandler<CDOEventArgs> BeforeCreate;
        public event EventHandler<CDOEventArgs> BeforeDelete;
        public event EventHandler<CDOEventArgs> BeforeFill;
        public event EventHandler<CDOEventArgs> BeforeInvoke;
        public event EventHandler<CDOEventArgs> BeforeRead;
        public event EventHandler<CDOEventArgs> BeforeSaveChanges;
        public event EventHandler<CDOEventArgs> BeforeUpdate;

        public ICloudDataRecord<T> Record { get; set; }
        public ICloudDataRecord<T> Add()
        {
            throw new NotImplementedException();
        }

        public ICloudDataRecord<T> Create()
        {
            throw new NotImplementedException();
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

        #region validation
        private void VerifyResourceName(string resource)
        {
            if (!_cDOSession.Services.Any(s => s.Resources.Any(r => r.Name.Equals(resource))))
            {
                throw new NotSupportedException($"Invalid resourcename {resource}.");
            }
        }
        #endregion
    }
}
