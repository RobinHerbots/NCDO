using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Json;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Reflection;
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
        private readonly CDOSessionOptions _options;

        #region Constructor

        public CDOSession(CDOSessionOptions options)
        {
            _options = options;
            Instance = this; //used by cdo when no session object is passed

            //init httpclient
            HttpClient = new HttpClient();
            //HttpClient = new HttpClient(new HttpClientHandler() { SslProtocols = _options.SslProtocols });  //this is not supported in older frameworks & problematic in Outlook VSTO
            ServicePointManager.SecurityProtocol = _options.SecurityProtocol;

            HttpClient.DefaultRequestHeaders.ConnectionClose = false;
            HttpClient.DefaultRequestHeaders.CacheControl = CacheControlHeaderValue.Parse("no-cache");
            HttpClient.DefaultRequestHeaders.Pragma.Add(NameValueHeaderValue.Parse("no-cache"));
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            Task.Factory.StartNew(() =>
                NetworkChange.NetworkAvailabilityChanged += NetworkChange_NetworkAvailabilityChanged).Wait();
        }

        #endregion

        /// <inheritdoc />
        public Uri ServiceURI => _options.ServiceUri;

        public string UserName => _options.ClientId;
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
                    if (_options.ClientId == null) throw new ArgumentNullException(nameof(_options.ClientId));
                    if (_options.ClientSecret == null) throw new ArgumentNullException(nameof(_options.ClientSecret));
                    return Convert.ToBase64String(
                        Encoding.UTF8.GetBytes($"{_options.ClientId}:{_options.ClientSecret}"));
                case AuthenticationModel.Bearer:
                    if (_options.ClientId == null) throw new ArgumentNullException(nameof(_options.ClientId));
                    if (_options.ClientSecret == null) throw new ArgumentNullException(nameof(_options.ClientSecret));
                    if (_options.Authority == null) throw new ArgumentNullException(nameof(_options.Authority));
                    if (_options.Audience == null) throw new ArgumentNullException(nameof(_options.Audience));
                    _authContext = new AuthenticationContext(_options.Authority, _tokenCache);
                    var clientCredential = new ClientCredential(_options.ClientId, _options.ClientSecret);
                    return (await _authContext.AcquireTokenAsync(_options.Audience, clientCredential)).AccessToken;
                case AuthenticationModel.Bearer_WIA:
                    if (_options.ClientId == null) throw new ArgumentNullException(nameof(_options.ClientId));
                    if (_options.Authority == null) throw new ArgumentNullException(nameof(_options.Authority));
                    if (_options.Audience == null) throw new ArgumentNullException(nameof(_options.Audience));
                    _authContext = new AuthenticationContext(_options.Authority, _tokenCache);
                    var userCredential = new UserCredential();
                    return (await _authContext.AcquireTokenAsync(_options.Audience, _options.ClientId, userCredential))
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
                    new AuthenticationHeaderValue(_options.Challenge, await GetChallengeToken());
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
                var urlBuilder = new StringBuilder(_options.ServiceUri.AbsoluteUri);
                using (var request = new HttpRequestMessage())
                {
                    await PrepareLoginRequest(request, urlBuilder);
                    await OnOpenRequest(HttpClient, request);
                    var response = await HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead,
                        cancellationToken);
                    await ProcessLoginResponse(response);
                }
            }
            catch (Exception)
            {
                //swallow
            }
            finally
            {
                //remove me when kevin fixes PAS
                Connected = true;
                LoginHttpStatus = HttpStatusCode.OK;
            }

            return LoginResult;
        }

#pragma warning disable 1998
        public virtual async Task ProcessLoginResponse(HttpResponseMessage response)
#pragma warning restore 1998
        {
            Connected = true;
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
            Connected = e.IsAvailable;
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

        public AuthenticationModel AuthenticationModel => _options.AuthenticationModel;
        public ICollection<Uri> CatalogURIs { get; } = new List<Uri>();
        public string ClientContextId { get; set; }
        public ICloudDataObject[] CDOs { get; set; }
        public bool Connected { get; private set; } = false;

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