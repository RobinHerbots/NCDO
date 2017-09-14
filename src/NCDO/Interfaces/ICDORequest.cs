using System;
using System.Collections.Generic;
using System.Json;
using System.Net.Http;
using System.Text;

namespace NCDO.Interfaces
{
    public interface ICDORequest
    {
        /// <summary>
        /// A reference to an object with a property named operations, which is an array containing the request objects for each of the one or more Data Object record-change operations performed in response to calling the CDO saveChanges( ) method either with an empty parameter list;
        /// </summary>
        IEnumerable<ICDORequest> Batch { get; }

        /// <summary>
        /// For an invoke operation, the name of the CDO invocation method that called the operation.
        /// </summary>
        string FnName { get; }

        /// <summary>
        /// An object reference to the CDO that performed the operation returning the request object.
        /// </summary>
        ICloudDataObject CDO { get; }
        /// <summary>
        /// An object reference to the record created, updated, or deleted by the current Data Object record-change operation.
        /// </summary>
        ICloudDataRecord Record { get; }
        /// <summary>
        /// A reference to the object, if any, that was passed as an input parameter to the CDO method that has returned the current request object.
        /// </summary>
        JsonObject ObjParam { get; }
        /// <summary>
        /// Returns an object whose properties contain data from a Data Object built-in or invoke operation executed on the Cloud Data Server.
        /// </summary>
        JsonObject Response { get; }
        /// <summary>
        /// A Boolean that when set to true indicates that the Data Object operation was successfully executed.
        /// </summary>
        bool? Success { get; }

        #region Passthrough props
        HttpResponseMessage ResponseMessage { get; }
        Uri RequestUri { get; }
        HttpMethod Method { get; }
        #endregion
    }
}
