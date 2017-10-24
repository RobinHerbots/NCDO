using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using NCDO.Definitions;
using NCDO.Interfaces;
using System.Json;
using NCDO.Catalog;

namespace NCDO
{
    /// <summary>
    /// Default anonymous session
    /// </summary>
    public partial class CDOSession : ICDOSession
    {
        #region Constructor

        public CDOSession(Uri serviceUri)
        {
            AuthenticationModel = Definitions.AuthenticationModel.AUTH_TYPE_ANON;
            ServiceURI = serviceUri;
            Instance = this; //used by cdo when no session object is passed

            //init httpclient
            HttpClient = new HttpClient();
            HttpClient.DefaultRequestHeaders.ConnectionClose = false;
            HttpClient.DefaultRequestHeaders.CacheControl = CacheControlHeaderValue.Parse("no-cache");
            HttpClient.DefaultRequestHeaders.Pragma.Add(NameValueHeaderValue.Parse("no-cache"));
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
        #endregion

        public string UserName { get; private set; }
        public HttpClient HttpClient { get; }

        private readonly Dictionary<Uri, ICDOCatalog> _catalogs = new Dictionary<Uri, ICDOCatalog>();
        public virtual void OnOpenRequest(HttpRequestMessage request)
        {
            //add authorization if needed
        }

        public async Task AddCatalog(Uri catalogUri, string userName = null, string password = null)
        {
            ThrowIfDisposed();
            if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
            {
                await Logout();
                await Login(userName, password);
            }
            if (!_catalogs.ContainsKey(catalogUri))
            {
                _catalogs.Add(catalogUri, await CDOCatalog.Load(catalogUri, this));
            }
        }

        public async Task AddCatalog(Uri[] catalogUris, string userName = null, string password = null)
        {
            ThrowIfDisposed();
            foreach (var catalogUri in catalogUris)
            {
                await AddCatalog(catalogUri, userName, password);
            }
        }

        protected virtual string _loginURI { get; } = "/static/home.html";

        public async Task Login(string userName = null, string password = null)
        {
            ThrowIfDisposed();
            if (AuthenticationModel != Definitions.AuthenticationModel.AUTH_TYPE_ANON)
            {
                if (userName == null) throw new ArgumentNullException(nameof(userName));
                if (password == null) throw new ArgumentNullException(nameof(password));
            }

            UserName = userName;

            var urlBuilder = new StringBuilder(ServiceURI.AbsoluteUri);
            using (var request = new HttpRequestMessage())
            {
                await PrepareLoginRequest(request, urlBuilder);
                var response = await HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                await ProcessLoginResponse(response);
            }
        }

        public virtual async Task ProcessLoginResponse(HttpResponseMessage response)
        {
            LoginHttpStatus = response.StatusCode;
        }

        public virtual async Task PrepareLoginRequest(HttpRequestMessage request, StringBuilder urlBuilder)
        {
            urlBuilder.Append(_loginURI);

            request.Method = new HttpMethod("GET");
            request.RequestUri = new Uri(urlBuilder.ToString(), UriKind.RelativeOrAbsolute);
        }

        public async Task Logout()
        {
            ThrowIfDisposed();
            throw new NotImplementedException();
        }

        public void Ping()
        {
            ThrowIfDisposed();
            throw new NotImplementedException();
        }

        public event EventHandler<CDOEventArgs> Online;
        public event EventHandler<CDOOfflineEventArgs> Offline;

        public string AuthenticationModel { get; }
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
        public Uri ServiceURI { get; }


        /// <summary>
        /// Latest initialized CDOSession
        /// </summary>
        public static ICDOSession Instance { get; internal set; }

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
