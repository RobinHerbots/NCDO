using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NCDO.Catalog;
using NCDO.Definitions;
using NCDO.Events;

namespace NCDO.Interfaces
{
    public interface ICDOSession : IDisposable
    {
        #region Properties
        /// <summary>
        /// A string constant that specifies the type of authentication
        ///that the server requires from the Mobile or Web App
        /// </summary>
        AuthenticationModel AuthenticationModel { get; }
        /// <summary>
        /// Returns the list of URIs used to load the CDO catalogs
        /// to access the Cloud Data Services provided by the
        ///  Mobile or Web application for which the current
        /// CDOSession object manages a user login session.
        /// </summary>
        ICollection<Uri> CatalogURIs { get; }
        /// <summary>
        /// The value of the most recent client context identifier (CCID) that the Session object has found in the
        ///X-CLIENT-CONTEXT-ID HTTP header of a server response message.
        /// </summary>
        string ClientContextId { get; }
        /// <summary>
        /// Returns an array of CDOs that use the current Session object to communicate with their Data Object services.
        /// </summary>
        ICloudDataObject[] CDOs { get; }
        /// <summary>
        /// Returns a Boolean that indicates the most recent online status of the current CDOSession object, when it last determined if the Mobile or Web application it manages is available.
        /// </summary>
        bool Connected { get; }

        /// <summary>
        /// Returns the specific HTTP status code returned in the response from the most recent login attempt on the current CDOSession object.
        /// </summary>
        HttpStatusCode LoginHttpStatus { get; }

        /// <summary>
        /// Returns the return value of the login( ) method, which is the basic result code for the most recent login attempt on the current CDOSession object.
        /// </summary>
        SessionStatus LoginResult { get; }
        /// <summary>
        /// Identifies the Cloud Data Services that have been loaded for the current CDOSession object and its Mobile or Web application.
        /// </summary>
        IEnumerable<Service> Services { get; }
        /// <summary>
        /// Returns the URI to the Mobile or Web application passed as a parameter to the constructor as a property of the constructor’s options property.
        /// </summary>
        Uri ServiceURI { get; }
        /// <summary>
        /// Returns the user ID passed as a parameter to the most recent call to the login( ) method on the current CDOSession object.
        /// </summary>
        string UserName { get; }
        HttpClient HttpClient { get; }
        CDOSessionOptions Options { get; }
        #endregion
        #region Methods
        /// <summary>
        /// Posthandle the response
        /// </summary>
        /// <param name="client"></param>
        /// <param name="response"></param>
        Task ProcessLoginResponse(HttpResponseMessage response);

        /// <summary>
        /// Prepare the login request.  Override to implement basic, form, SSO
        /// </summary>
        /// <param name="client"></param>
        /// <param name="request"></param>
        /// <param name="urlBuilder"></param>
        Task PrepareLoginRequest(HttpRequestMessage request, StringBuilder urlBuilder);

        /// <summary>
        /// Used to modify a request object before sending the request object to the server.
        /// Use for adding authorization to the operations and invokes
        /// </summary>
        /// <param name="client"></param>
        /// <param name="request"></param>
        Task OnOpenRequest(HttpClient client, HttpRequestMessage request);

        /// <summary>
        /// Loads a CDO catalog for a login session established using the login( ) method.
        /// </summary>
        Task AddCatalog(params Uri[] catalogUris);
        /// <summary>
        /// Starts a user login session on the current CDOSession object by sending an HTTP request with user credentials to a URI for a specified Mobile or Web application.
        /// </summary>
        Task<SessionStatus> Login(CancellationToken cancellationToken = default(CancellationToken));
        /// <summary>
        /// Terminates the login session on the Mobile or Web application managed by the current CDOSession object, and invalidates any session currently maintained by the server.
        /// </summary>
        Task Logout();

        #endregion
        #region Events
        /// <summary>
        /// Fires when the current CDOSession object detects that the device on which it is running has gone offline, or that the Mobile or Web application to which it has been connected is no longer available.
        /// </summary>
        event EventHandler<CDOEventArgs> Online;
        /// <summary>
        /// Fires when the current CDOSession object detects that the device on which it is running has gone online after it was previously offline, or that the Mobile or Web application to which it is connected is now available after it was previously unavailable.
        /// </summary>
        event EventHandler<CDOOfflineEventArgs> Offline;

        #endregion
    }
}