using System;
using System.Collections.Generic;
using System.Json;
using System.Net.Http;
using System.Text;
using NCDO.Extensions;
using NCDO.Interfaces;

namespace NCDO
{
    public class CDORequest : ICDORequest
    {
        public IEnumerable<ICDORequest> Batch { get; internal set; }
        public string FnName { get; internal set; }
        public ICloudDataObject CDO { get; internal set; }
        public ICloudDataRecord Record { get; internal set; }
        public JsonObject ObjParam { get; internal set; }

        public JsonObject Response { get; internal set; }

        public bool? Success { get; internal set; }

        public HttpResponseMessage ResponseMessage { get; internal set; }
        public Uri RequestUri { get; internal set; }
        public HttpMethod Method { get; internal set; }

        public void ThrowOnError()
        {
            Response?.ThrowOnErrorResponse();
            if (Success.HasValue && !Success.Value)
                throw new CDOException($"{ResponseMessage.StatusCode} - {ResponseMessage.RequestMessage.RequestUri}");
        }
    }
}
