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
    public interface ICloudDataObject : IDisposable
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
        Task<ICDORequest> Fill(QueryRequest queryRequest = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        ///     Initializes CDO memory with record objects from the data
        ///     records in a single table, or in one or more tables of a
        ///     ProDataSet, as returned by the built-in read operation of the
        ///     resource for which the CDO is created.
        /// </summary>
        Task<ICDORequest> Fill(string filter, CancellationToken cancellationToken = default(CancellationToken));


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
        Task<ICDORequest> Invoke(string operation, JsonObject inputObject = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        ///     Initializes CDO memory with record objects from the data
        ///     records in a single table, or in one or more tables of a
        ///     ProDataSet, as returned by the built-in read operation of the
        ///     resource for which the CDO is created.
        /// </summary>
        Task<ICDORequest> Read(QueryRequest queryRequest = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        ///     Initializes CDO memory with record objects from the data
        ///     records in a single table, or in one or more tables of a
        ///     ProDataSet, as returned by the built-in read operation of the
        ///     resource for which the CDO is created.
        /// </summary>
        Task<ICDORequest> Read(string filter, CancellationToken cancellationToken = default(CancellationToken));

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


        Task ProcessCRUDResponse(HttpResponseMessage response, CDORequest request, CancellationToken cancellationToken = default(CancellationToken));
        Task ProcessInvokeResponse(HttpResponseMessage response, CDORequest request, CancellationToken cancellationToken = default(CancellationToken));

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
        Task<ICDORequest> Fill(Expression<Func<R, bool>> filter, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        ///     Searches for a record in a table referenced in CDO memory
        ///     and returns a reference to that record if found. If no record
        ///     is found, it returns null.
        /// </summary>
        /// <returns></returns>
        Task<R> Find(Expression<Func<R, bool>> filter, bool autoFetch, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        ///     Searches for a record in a table
        ///     and returns a reference to a dataset with all related data from the record if found. If no record
        ///     is found, it returns null.
        /// </summary>
        /// <returns></returns>
        Task<D> Get(Expression<Func<R, bool>> filter, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        ///     Locates and returns the record in CDO memory with the
        ///     internal ID you specify.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<R> FindById(string id, bool autoFetch, CancellationToken cancellationToken = default(CancellationToken));

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
        Task<ICDORequest> Read(Expression<Func<R, bool>> filter, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        ///     Deletes the specified table record referenced in CDO
        ///     memory.
        /// </summary>
        bool Remove(R record);

        /// <summary>
        /// Reset CDO Memory
        /// </summary>
        void Reset();

        /// <summary>
        ///     Synchronizes to the Cloud Data Server all changes pending in
        ///     CDO memory since the last call to the fill( ) or
        ///     saveChanges( ) methods, or since any prior changes
        ///     have been otherwise accepted or rejected.
        /// </summary>
        Task SaveChanges(CDO_Table<R> tableRef = null, CancellationToken cancellationToken = default(CancellationToken));

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
}