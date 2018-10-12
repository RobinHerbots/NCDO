using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using NCDO.Definitions;
using NCDO.Interfaces;
using System.Json;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Cryptography;
using NCDO.Catalog;
using NCDO.Events;

namespace NCDO
{
    /// <summary>
    /// CDOSession
    /// </summary>
    public partial class CDOSession : ICDOSession
    {
        private CDOSessionOptions _options;

        #region Constructor

        public CDOSession(CDOSessionOptions options)
        {
            _options = options;
            Instance = this; //used by cdo when no session object is passed

            //init httpclient
            if (_options.SslProtocols == SslProtocols.Default) HttpClient = new HttpClient();
            else
            {
                try
                {
                    HttpClient = new HttpClient(new HttpClientHandler() { SslProtocols = _options.SslProtocols });
                }
                catch (PlatformNotSupportedException ex)
                {
                    HttpClient = new HttpClient();
                    if (Enum.TryParse(_options.SslProtocols.ToString(), out SecurityProtocolType spt))
                        ServicePointManager.SecurityProtocol = spt;
                }
            }

            HttpClient.DefaultRequestHeaders.ConnectionClose = false;
            HttpClient.DefaultRequestHeaders.CacheControl = CacheControlHeaderValue.Parse("no-cache");
            HttpClient.DefaultRequestHeaders.Pragma.Add(NameValueHeaderValue.Parse("no-cache"));
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
        #endregion

        /// <inheritdoc />
        public Uri ServiceURI => _options.ServiceUri;
        public string UserName => _options.ClientId;
        public HttpClient HttpClient { get; }

        private readonly Dictionary<Uri, ICDOCatalog> _catalogs = new Dictionary<Uri, ICDOCatalog>();
#pragma warning disable 1998
        public virtual async Task OnOpenRequest(HttpClient client, HttpRequestMessage request)
#pragma warning restore 1998
        {
            //add authorization if needed
            if (AuthenticationModel != AuthenticationModel.Anonymous)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue(_options.Challenge, _options.ChallengeToken);
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

        protected virtual string _loginURI { get; } = "/static/home.html";

        public async Task Login()
        {
            ThrowIfDisposed();

            var urlBuilder = new StringBuilder(_options.ServiceUri.AbsoluteUri);
            using (var request = new HttpRequestMessage())
            {
                await PrepareLoginRequest(request, urlBuilder);
                await OnOpenRequest(HttpClient, request);
                var response = await HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                await ProcessLoginResponse(response);
            }
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

        public void Ping()
        {
            ThrowIfDisposed();
            throw new NotImplementedException();
        }

        public event EventHandler<CDOEventArgs> Online;
        public event EventHandler<CDOOfflineEventArgs> Offline;

        public AuthenticationModel AuthenticationModel => _options.AuthenticationModel;
        public ICollection<Uri> CatalogURIs { get; } = new List<Uri>();
        public string ClientContextId { get; set; }
        public ICloudDataObject[] CDOs { get; set; }
        public bool Connected => LoginHttpStatus == HttpStatusCode.OK;
        public HttpStatusCode LoginHttpStatus { get; private set; } = HttpStatusCode.Ambiguous;

        public SessionStatus LoginResult
        {
            get
            {
                switch (LoginHttpStatus)
                {
                    case HttpStatusCode.OK:
                        return SessionStatus.AUHTENTICATION_SUCCESS;
                    case HttpStatusCode.Unauthorized:
                        return SessionStatus.AUTHENTICATION_FAILURE;
                    default:
                        return SessionStatus.GENERAL_FAILURE;
                }
            }
        }

        public int PingInterval { get; set; }
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

            HttpClient?.Dispose();
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
