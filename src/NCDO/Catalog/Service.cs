using System;
using System.Collections.Generic;
using System.Json;
using NCDO.Extensions;

namespace NCDO.Catalog
{
    public class Service
    {
        private readonly JsonValue _serviceDefinition;
        private List<Resource> _resources = null;

        internal Service(JsonValue serviceDefinition)
        {
            _serviceDefinition = serviceDefinition;
        }

        /// <summary>
        /// Name of the service
        /// </summary>
        public String Name => _serviceDefinition.Get("name");

        /// <summary>
        /// Relative service URI
        /// </summary>
        public Uri Address => new Uri(_serviceDefinition.Get("address"), UriKind.Relative);

        /// <summary>
        /// Specifies whether a request object should be used for
        ///invoke operations. Default value is false.
        ///OE backends require this to be set to true.
        /// </summary>
        public bool UseRequest => _serviceDefinition.Get("useRequest");

        /// <summary>
        /// Optional.If true, tells CDO to only send the fields that
        ///change back to the server. If false, CDO will send the
        ///whole record. Default is false.
        /// </summary>
        public bool SendOnlyChanges => _serviceDefinition.Get("sendOnlyChanges");

        /// <summary>
        /// Optional. Indicates whether the server expects an
        ///enclosing object of the same name when the CDO sends
        ///parameters.
        /// </summary>
        public bool UnWrapped => _serviceDefinition.Get("unWrapped");

        /// <summary>
        /// Optional. Indicates whether or not the backend expects a
        ///clientProps header with context for every operation.
        /// </summary>
        public bool UseXClientProps => _serviceDefinition.Get("useXClientProps");

        /// <summary>
        /// Optional. Represents the customer id.
        /// </summary>
        public string TenantId => _serviceDefinition.Get("tenantId");

        /// <summary>
        /// Optional. Represents the application id.
        /// </summary>
        public string ApplicationId => _serviceDefinition.Get("appId");

        /// <summary>
        /// Available Resources
        /// </summary>
        public List<Resource> Resources
        {
            get
            {
                if (_resources == null)
                {
                    _resources = new List<Resource>();
                    foreach (JsonValue resource in _serviceDefinition.Get("resources"))
                    {
                        _resources.Add(new Resource(resource));
                    }
                }

                return _resources;
            }
        }
    }
}