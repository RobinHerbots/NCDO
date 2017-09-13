using NCDO.Extensions;
using System;
using System.Collections.Generic;
using System.Json;

namespace NCDO.Catalog
{
    public class Resource
    {
        public Resource(JsonValue resource)
        {
            Name = resource.Get("name");
            Path = new Uri(resource.Get("path"), UriKind.Relative);
            AutoSave = resource.Get("autoSave");
            IdProperty = resource.Get("idProperty");
            DisplayName = resource.Get("displayName");
            foreach (JsonValue operation in resource.Get("operations"))
            {
                Operations.Add(new Operation(operation));
            }
            if (resource.ContainsKey("relations"))
            {
                Relations = new List<Relation>();
                foreach (JsonValue relation in resource.Get("relations"))
                {
                    Relations.Add(new Relation(relation));
                }
            }
            if (resource.ContainsKey("schema"))
            {
                Schema = new Schema(resource.Get("schema"));
            }
            if (resource.ContainsKey("dataDefinitions"))
            {
                DataDefinitions = new List<DataDefinition>();
                JsonObject dataDefinitions = (JsonObject)resource.Get("dataDefinitions");
                foreach (var key in dataDefinitions.Keys)
                {
                    DataDefinitions.Add(new DataDefinition(dataDefinitions[key]));
                }
            }
        }
        /// <summary>
        /// Name of resource containing business logic
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Unique resource URI, relative to its service address
        /// </summary>
        public Uri Path { get; }
        public bool AutoSave { get; }
        /// <summary>
        /// Optional. Indicates to the CDO the name of the field in the
        ///schema that should be used to identify a record.
        ///This has been added because some types of services
        ///have a specific property (field) to identify records.
        /// 
        /// If specified, it is sent to the server as part of a Create,
        ///Update, or Delete operation. It is also used to match
        ///records when merging data from an Invoke operation into
        ///the CDO memory.
        /// </summary>
        public string IdProperty { get; }
        /// <summary>
        /// Optional. A client tool can use this for display purposes in
        ///places where a name for a resource needs to be shown. If
        ///not specified, client tools can use the resource’s “name”
        ///property.
        /// </summary>
        public string DisplayName { get; }
        /// <summary>
        /// The Operations property is required. It is an array that provides information about each operation that the Mobile resource supports, information such as how the operation’s parameters are mapped. This information from the catalog is used by the CDO to construct the HTTP request to be sent to the REST adapter. So, instead of a client app having to identify the URI, send an HTTP request, and interpret the HTTP response for a given REST resource operation call, it only has to call the appropriate method on the CDO to execute the corresponding operation on the resource.
        /// </summary>
        public List<Operation> Operations { get; } = new List<Operation>();
        /// <summary>
        /// The Relations property is optional. It should only be provided for DataSets with more than 1 table specified in the schema. It is an array that contains a “relations” entry for each relationship in the DataSet hierarchy
        /// </summary>
        public List<Relation> Relations { get; }

        /// <summary>
        /// The Schema property provides the data definition to be used by the built-in CRUD operations.
        /// </summary>
        public Schema Schema { get; }
        /// <summary>
        /// The DataDefinitions property provides the data definition to be used by the invoke operations.
        /// /// </summary>
        public List<DataDefinition> DataDefinitions { get; }
    }
}
