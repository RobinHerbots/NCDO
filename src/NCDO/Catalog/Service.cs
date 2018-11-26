using System;
using System.Collections.Generic;
using System.Json;
using NCDO.Extensions;

namespace NCDO.Catalog
{
    public class Service
    {
        internal Service(JsonValue serviceDefinition)
        {
            Name = serviceDefinition.Get("name");
            Address = new Uri(serviceDefinition.Get("address"), UriKind.Relative);
            UseRequest = serviceDefinition.Get("useRequest");
            SendOnlyChanges = serviceDefinition.Get("sendOnlyChanges");
            UnWrapped = serviceDefinition.Get("unWrapped");
            UseXClientProps = serviceDefinition.Get("useXClientProps");
            TenantId = serviceDefinition.Get("tenantId");
            ApplicationId = serviceDefinition.Get("appId");
            foreach (JsonValue resource in serviceDefinition.Get("resources"))
            {
                Resources.Add(new Resource(resource));
            }
        }

        /// <summary>
        /// Name of the service
        /// </summary>
        public String Name { get; }
        /// <summary>
        /// Relative service URI
        /// </summary>
        public Uri Address { get; }

        /// <summary>
        /// Specifies whether a request object should be used for
        ///invoke operations. Default value is false.
        ///OE backends require this to be set to true.
        /// </summary>
        public bool UseRequest { get; } = false;

        /// <summary>
        /// Optional.If true, tells CDO to only send the fields that
        ///change back to the server. If false, CDO will send the
        ///whole record. Default is false.
        /// </summary>
        public bool SendOnlyChanges { get; } = false;

        /// <summary>
        /// Optional. Indicates whether the server expects an
        ///enclosing object of the same name when the CDO sends
        ///parameters.
        /// </summary>
        public bool UnWrapped { get; } = false;
        /// <summary>
        /// Optional. Indicates whether or not the backend expects a
        ///clientProps header with context for every operation.
        /// </summary>
        public bool UseXClientProps { get; } = false;
        /// <summary>
        /// Optional. Represents the customer id.
        /// </summary>
        public string TenantId { get; }
        /// <summary>
        /// Optional. Represents the application id.
        /// </summary>
        public string ApplicationId { get; }
        /// <summary>
        /// Available Resources
        /// </summary>
        public List<Resource> Resources { get; } = new List<Resource>();
    }
}
