using System;
using System.Collections.Generic;
using System.Json;
using System.Net.Http;
using System.Text;
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
    }
}
