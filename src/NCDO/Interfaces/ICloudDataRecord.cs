using System.ComponentModel;

namespace NCDO.Interfaces
{
    /// <summary>
    /// CloudDataRecord is a record instance for any table stored in the local memory of an associated class instance (CDO).
    /// </summary>
    public interface ICloudDataRecord : INotifyPropertyChanged, INotifyPropertyChanging
    {
        #region Properties
        /// <summary>
        /// The data (field values) for a record.
        /// </summary>
        System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, System.Json.JsonValue>> Data { get; }
        #endregion

        #region Methods
        /// <summary>
        /// Accepts changes to the data in CDO memory for a specified record object.
        /// </summary>
        void AcceptRowChanges();
        /// <summary>
        /// Updates field values for the specified CloudDataRecord object in CDO memory.
        /// </summary>
        void Assign();
        /// <summary>
        /// Returns any before-image error string in the data of a record
        /// object referenced in CDO memory that was set as the result
        /// of a Data Object create, update, delete, or submit operation.
        /// </summary>
        /// <returns></returns>
        string GetErrorString();
        /// <summary>
        /// Returns the unique internal ID for the record object
        /// referenced in CDO memory.
        /// </summary>
        /// <returns></returns>
        string GetId();
        /// <summary>
        /// Deletes the specified table record referenced in CDO memory.
        /// </summary>
        void Remove();
        /// <summary>
        /// Rejects changes to the data in CDO memory for a specified
        /// record object.
        /// </summary>
        void RejectRowChanges();
        /// <summary>
        /// Indicates if a property is changed
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        bool IsPropertyChanged(string propertyName);

        #endregion
    }
} 
