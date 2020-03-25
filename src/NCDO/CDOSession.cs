using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Json;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NCDO.Catalog;
using NCDO.Definitions;
using NCDO.Events;
using NCDO.Interfaces;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace NCDO
{
    /// <summary>
    /// CDOSession
    /// </summary>
    public partial class CDOSession : ICDOSession
    {
        #region Constructor

        public CDOSession(CDOSessionOptions options)
        {
            Options = options;
            Instance = this; //used by cdo when no session object is passed

            //init httpclient
            HttpClient = new HttpClient(new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
            });
            //HttpClient = new HttpClient(new HttpClientHandler() { SslProtocols = _options.SslProtocols });  //this is not supported in older frameworks & problematic in Outlook VSTO
            ServicePointManager.SecurityProtocol = Options.SecurityProtocol;

            HttpClient.DefaultRequestHeaders.ConnectionClose = false;
            HttpClient.DefaultRequestHeaders.CacheControl = CacheControlHeaderValue.Parse("no-cache");
            HttpClient.DefaultRequestHeaders.Pragma.Add(NameValueHeaderValue.Parse("no-cache"));
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            Task.Factory.StartNew(() =>
                NetworkChange.NetworkAvailabilityChanged += NetworkChange_NetworkAvailabilityChanged).Wait();
        }

        #endregion

        public CDOSessionOptions Options { get; private set; }

        /// <inheritdoc />
        public Uri ServiceURI => Options.ServiceUri;

        public string UserName => Options.ClientId;
        public HttpClient HttpClient { get; }

        private readonly Dictionary<Uri, ICDOCatalog> _catalogs = new Dictionary<Uri, ICDOCatalog>();


        private readonly TokenCache _tokenCache = new TokenCache();
        private AuthenticationContext _authContext;

        /// <summary>
        /// Internal ~ Returns the token to pass in the headers for the given challenge
        /// </summary>
        private async Task<string> GetChallengeToken()
        {
            switch (AuthenticationModel)
            {
                case AuthenticationModel.Basic:
                    if (Options.ClientId == null) throw new ArgumentNullException(nameof(Options.ClientId));
                    if (Options.ClientSecret == null) throw new ArgumentNullException(nameof(Options.ClientSecret));
                    return Convert.ToBase64String(
                        Encoding.UTF8.GetBytes($"{Options.ClientId}:{Options.ClientSecret}"));
                case AuthenticationModel.Bearer:
                case AuthenticationModel.Bearer_OnBehalf:
                    if (Options.ClientId == null) throw new ArgumentNullException(nameof(Options.ClientId));
                    if (Options.ClientSecret == null) throw new ArgumentNullException(nameof(Options.ClientSecret));
                    if (Options.Authority == null) throw new ArgumentNullException(nameof(Options.Authority));
                    if (Options.Audience == null) throw new ArgumentNullException(nameof(Options.Audience));
                    _authContext = new AuthenticationContext(Options.Authority, _tokenCache);
                    var clientCredential = new ClientCredential(Options.ClientId, Options.ClientSecret);

                    if (AuthenticationModel == AuthenticationModel.Bearer_OnBehalf)
                    {
                        var tokenAssertion = Options.UserAccessToken.Invoke();
                        if (tokenAssertion != null)
                        {
                            UserAssertion userAssertion = new UserAssertion(tokenAssertion,
                                "urn:ietf:params:oauth:grant-type:jwt-bearer", Options.UserName.Invoke());
                            return (await _authContext.AcquireTokenAsync(Options.Audience, clientCredential,
                                    userAssertion))
                                .AccessToken;
                        }
                    }

                    return (await _authContext.AcquireTokenAsync(Options.Audience, clientCredential)).AccessToken;

                case AuthenticationModel.Bearer_WIA:
                    if (Options.ClientId == null) throw new ArgumentNullException(nameof(Options.ClientId));
                    if (Options.Authority == null) throw new ArgumentNullException(nameof(Options.Authority));
                    if (Options.Audience == null) throw new ArgumentNullException(nameof(Options.Audience));
                    _authContext = new AuthenticationContext(Options.Authority, _tokenCache);
                    var userCredential = new UserCredential();
                    return (await _authContext.AcquireTokenAsync(Options.Audience, Options.ClientId, userCredential))
                        .AccessToken;
            }

            return null;
        }

#pragma warning disable 1998
        public virtual async Task OnOpenRequest(HttpClient client, HttpRequestMessage request)
#pragma warning restore 1998
        {
            //add authorization if needed
            if (AuthenticationModel != AuthenticationModel.Anonymous)
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue(Options.Challenge, await GetChallengeToken());
            }
        }

        public async Task AddCatalog(params Uri[] catalogUris)
        {
            ThrowIfDisposed();
            foreach (var catalogUri in catalogUris)
            {
                if (!_catalogs.ContainsKey(catalogUri))
                {
                    _catalogs.Add(catalogUri, await CDOCatalog.Load(catalogUri, this));
                }
            }
        }

        public void LoadEmbeddedCatalog(Assembly assembly, string catalogResource)
        {
            ThrowIfDisposed();
            using (var stream = assembly.GetManifestResourceStream(catalogResource))
            {
                var catalogUri = new Uri($"file://{catalogResource}");
                if (!_catalogs.ContainsKey(catalogUri))
                {
                    _catalogs.Add(catalogUri, CDOCatalog.Load((JsonObject) JsonValue.Load(stream), this));
                }
            }
        }

        protected virtual string _loginURI { get; } = "/static/home.html";

        public async Task<SessionStatus> Login(CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            try
            {
                if (Options.AuthenticationModel != AuthenticationModel.Bearer_OnBehalf ||
                    !string.IsNullOrEmpty(Options.UserName.Invoke()))
                {
                    var urlBuilder = new StringBuilder(Options.ServiceUri.AbsoluteUri);
                    using (var request = new HttpRequestMessage())
                    {
                        await PrepareLoginRequest(request, urlBuilder);
                        await OnOpenRequest(HttpClient, request);
                        var response = await HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead,
                            cancellationToken);
                        await ProcessLoginResponse(response);
                    }
                }
            }
            catch (Exception)
            {
                //swallow
            }

            return LoginResult;
        }

#pragma warning disable 1998
        public virtual async Task ProcessLoginResponse(HttpResponseMessage response)
#pragma warning restore 1998
        {
            LoginHttpStatus = response.StatusCode;
        }

#pragma warning disable 1998
        public virtual async Task PrepareLoginRequest(HttpRequestMessage request, StringBuilder urlBuilder)
#pragma warning restore 1998
        {
            urlBuilder.Append(_loginURI);

            request.Method = new HttpMethod("GET");
            request.RequestUri = new Uri(urlBuilder.ToString(), UriKind.RelativeOrAbsolute);
        }

#pragma warning disable 1998
        public async Task Logout()
#pragma warning restore 1998
        {
            ThrowIfDisposed();
            //throw new NotImplementedException(); 
        }

        private void NetworkChange_NetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
        {
            if (e.IsAvailable)
            {
                Login().Wait();
                Online?.Invoke(this, new CDOEventArgs() {Session = this});
            }
            else
            {
                LoginHttpStatus = HttpStatusCode.ServiceUnavailable;
                Offline?.Invoke(this, new CDOOfflineEventArgs() {Session = this});
            }
        }

        public event EventHandler<CDOEventArgs> Online;
        public event EventHandler<CDOOfflineEventArgs> Offline;

        public AuthenticationModel AuthenticationModel => Options.AuthenticationModel;
        public ICollection<Uri> CatalogURIs { get; } = new List<Uri>();
        public string ClientContextId { get; set; }
        public ICloudDataObject[] CDOs { get; set; }

        public bool Connected => NetworkInterface.GetIsNetworkAvailable();

        public HttpStatusCode LoginHttpStatus { get; private set; } = HttpStatusCode.Ambiguous;

        public SessionStatus LoginResult
        {
            get
            {
                switch (LoginHttpStatus)
                {
                    case HttpStatusCode.OK:
                        return SessionStatus.AUTHENTICATION_SUCCESS;
                    case HttpStatusCode.Unauthorized:
                        return SessionStatus.AUTHENTICATION_FAILURE;
                    default:
                        return SessionStatus.GENERAL_FAILURE;
                }
            }
        }

        public IEnumerable<Service> Services
        {
            get
            {
                var services = new List<Service>();
                foreach (var catalog in _catalogs)
                {
                    services.AddRange(catalog.Value.Services);
                }

                return services;
            }
        }

        /// <summary>
        /// Latest initialized CDOSession
        /// </summary>
        public static ICDOSession Instance { get; internal set; }

        /// <summary>
        /// Returns the version of NCDO
        /// Can be used by the clientgen to see if the ncdo version is supported.
        /// </summary>
        public static string Version
        {
            get
            {
                var assembly = Assembly.GetExecutingAssembly();
                var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                return fileVersionInfo.ProductVersion;
            }
        }

        #region Catalog Extensions

        /// <summary>
        ///     Verify if the resource is available and return the catalog definition for the catalog
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        public (Service service, Resource resource) VerifyResourceName(string resource)
        {
            var serviceDefinition = Services.FirstOrDefault(s => s.Resources.Any(r => r.Name.Equals(resource)));
            var resourceDefinition = serviceDefinition?.Resources.FirstOrDefault(r => r.Name.Equals(resource));
            if (resourceDefinition == null)
                throw new NotSupportedException($"Invalid resource name {resource}.");
            return (serviceDefinition, resourceDefinition);
        }

        public Operation VerifyOperation(string resource, string operation,
            OperationType operationType = OperationType.Invoke)
        {
            var resourceDefinition = VerifyResourceName(resource).resource;
            var operationDefinition = resourceDefinition?.Operations.FirstOrDefault(o =>
                o.Type == operationType && (string.IsNullOrEmpty(operation) || o.Name.Equals(operation)));
            if (operationDefinition == null)
                throw new NotSupportedException($"Invalid {operationType} operation {operation}.");
            return operationDefinition;
        }

        public string DetermineMainTable(string resource)
        {
            var resourceDefinition = VerifyResourceName(resource).resource;

            return resourceDefinition.Relations != null && resourceDefinition.Relations.Count > 0
                ? resourceDefinition.Relations.FirstOrDefault().ParentName
                : resourceDefinition.Schema?.Properties.FirstOrDefault().Value.Properties.FirstOrDefault().Key;
        }


        public string DeterminePrimaryKey(string resource, string tableName)
        {
            var resourceDefinition = VerifyResourceName(resource).resource;

            return resourceDefinition.Schema?.Properties.FirstOrDefault().Value.Properties[tableName]
                .PrimaryKey.FirstOrDefault();
        }

        #endregion


        #region IDisposable Support

        ~CDOSession()
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

//            HttpClient?.Dispose(); //https://aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/
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