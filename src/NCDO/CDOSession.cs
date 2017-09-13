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
        }
        #endregion

        public string UserName { get; private set; }

        private readonly Dictionary<Uri, ICDOCatalog> _catalogs = new Dictionary<Uri, ICDOCatalog>();
        public void OnOpenRequest(HttpClient client, HttpRequestMessage request)
        {
            request.Headers.TryAddWithoutValidation("Cache-Control", "no-cache");
            request.Headers.TryAddWithoutValidation("Pragma", "no-cache");
            //add authorization if needed
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task AddCatalog(Uri catalogUri, string userName = null, string password = null)
        {
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
            foreach (var catalogUri in catalogUris)
            {
                await AddCatalog(catalogUri, userName, password);
            }
        }

        protected virtual string _loginURI { get; } = "/static/home.html";

        public async Task Login(string userName = null, string password = null)
        {
            if (AuthenticationModel != Definitions.AuthenticationModel.AUTH_TYPE_ANON)
            {
                if (userName == null) throw new ArgumentNullException(nameof(userName));
                if (password == null) throw new ArgumentNullException(nameof(password));
            }

            UserName = userName;

            var urlBuilder = new StringBuilder(ServiceURI.AbsoluteUri);
            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage())
                {
                    await PrepareRequest(client, request, urlBuilder);
                    var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                    await ProcessResponse(client, response);
                }
            }
        }

        public virtual async Task ProcessResponse(HttpClient client, HttpResponseMessage response)
        {
            LoginHttpStatus = response.StatusCode;
        }

        public virtual async Task PrepareRequest(HttpClient client, HttpRequestMessage request, StringBuilder urlBuilder)
        {
            urlBuilder.Append(_loginURI);

            request.Method = new HttpMethod("GET");
            request.Headers.TryAddWithoutValidation("Cache-Control", "no-cache");
            request.Headers.TryAddWithoutValidation("Pragma", "no-cache");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.RequestUri = new Uri(urlBuilder.ToString(), UriKind.RelativeOrAbsolute);
        }

        public async Task Logout()
        {
            throw new NotImplementedException();
        }

        public void Ping()
        {
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
    }
}
