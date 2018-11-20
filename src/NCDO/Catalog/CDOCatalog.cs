using System;
using System.Collections.Generic;
using System.IO;
using System.Json;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using NCDO.Interfaces;
using NCDO.Extensions;

namespace NCDO.Catalog
{
    public class CDOCatalog : ICDOCatalog
    {
        private Uri _catalogUri;
        private CDOSession _cDOSession;

        private CDOCatalog(Uri catalogUri, CDOSession cDOSession)
        {
            this._catalogUri = catalogUri;
            this._cDOSession = cDOSession;
        }

        private CDOCatalog(JsonObject catalog, CDOSession cDOSession)
        {
            this._cDOSession = cDOSession;
            ProcessCatalogObject(catalog);
        }

        private async Task<ICDOCatalog> Load(CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            using (var request = new HttpRequestMessage())
            {
                request.Method = new HttpMethod("GET");
                await _cDOSession.OnOpenRequest(_cDOSession.HttpClient, request);
                request.RequestUri = _catalogUri;

                var response = await _cDOSession.HttpClient.SendAsync(request,
                    HttpCompletionOption.ResponseContentRead, cancellationToken);
                await ProcessResponse(_cDOSession.HttpClient, response, cancellationToken);
            }

            return this;
        }

        public virtual async Task ProcessResponse(HttpClient client, HttpResponseMessage response, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if ((response.Headers.TransferEncodingChunked.HasValue && response.Headers.TransferEncodingChunked.Value) ||
                response.Content.Headers.ContentLength > 0)
            {
                using (var dataStream = await response.Content.ReadAsStreamAsync())
                {
                    if (dataStream != null)
                    {
                        ProcessCatalogObject(JsonValue.Load(dataStream), cancellationToken);
                    }
                }
            }
        }
        private void ProcessCatalogObject(JsonValue catalogObject, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (catalogObject == null) throw new InvalidDataException("Could not parse catalog data.");
            catalogObject.ThrowOnErrorResponse();
            //init catalog from response
            Version = catalogObject.Get("version");
            LastModified = catalogObject.Get("lastModified");
            foreach (JsonValue serviceDefinition in catalogObject.Get("services"))
            {
                Services.Add(new Service(serviceDefinition));
            }

        }

        public static async Task<ICDOCatalog> Load(Uri catalogUri, CDOSession cDOSession, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await new CDOCatalog(catalogUri, cDOSession).Load(cancellationToken);
        }

        public static async Task<ICDOCatalog> Load(JsonObject catalog, CDOSession cDOSession, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            return new CDOCatalog(catalog, cDOSession);
        }

        public string Version { get; private set; }
        public string LastModified { get; private set; }
        public IList<Service> Services { get; } = new List<Service>();
    }
}
