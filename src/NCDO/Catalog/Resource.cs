using System;
using System.Collections.Generic;
using System.Json;
using NCDO.Extensions;

namespace NCDO.Catalog
{
    public class Resource
    {
        private JsonValue _resource;
        private List<Operation> _operations;
        private List<Relation> _relations;
        private Schema _schema;
        private List<DataDefinition> _dataDefinitions;

        public Resource(JsonValue resource)
        {
            _resource = resource;
        }

        /// <summary>
        /// Name of resource containing business logic
        /// </summary>
        public string Name => _resource.Get("name");

        /// <summary>
        /// Unique resource URI, relative to its service address
        /// </summary>
        public Uri Path => new Uri(_resource.Get("path"), UriKind.Relative);

        public bool AutoSave => _resource.Get("autoSave");

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
        public string IdProperty => _resource.Get("idProperty");

        /// <summary>
        /// Optional. A client tool can use this for display purposes in
        ///places where a name for a resource needs to be shown. If
        ///not specified, client tools can use the resource’s “name”
        ///property.
        /// </summary>
        public string DisplayName => _resource.Get("displayName");

        /// <summary>
        /// The Operations property is required. It is an array that provides information about each operation that the Mobile resource supports, information such as how the operation’s parameters are mapped. This information from the catalog is used by the CDO to construct the HTTP request to be sent to the REST adapter. So, instead of a client app having to identify the URI, send an HTTP request, and interpret the HTTP response for a given REST resource operation call, it only has to call the appropriate method on the CDO to execute the corresponding operation on the resource.
        /// </summary>
        public List<Operation> Operations
        {
            get
            {
                if (_operations == null)
                {
                    _operations = new List<Operation>();
                    foreach (JsonValue operation in _resource.Get("operations"))
                    {
                        _operations.Add(new Operation(operation));
                    }
                }

                return _operations;
            }
        }

        /// <summary>
        /// The Relations property is optional. It should only be provided for DataSets with more than 1 table specified in the schema. It is an array that contains a “relations” entry for each relationship in the DataSet hierarchy
        /// </summary>
        public List<Relation> Relations
        {
            get
            {
                if (_relations == null)
                {
                    _relations = new List<Relation>();
                    if (_resource.ContainsKey("relations"))
                    {
                        foreach (JsonValue relation in _resource.Get("relations"))
                        {
                            _relations.Add(new Relation(relation));
                        }
                    }
                }

                return _relations;
            }
        }

        /// <summary>
        /// The Schema property provides the data definition to be used by the built-in CRUD operations.
        /// </summary>
        public Schema Schema
        {
            get
            {
                if (_schema == null)
                {
                    if (_resource.ContainsKey("schema"))
                    {
                        _schema = new Schema(_resource.Get("schema"));
                    }
                }

                return _schema;
            }
        }

        /// <summary>
        /// The DataDefinitions property provides the data definition to be used by the invoke operations.
        /// /// </summary>
        public List<DataDefinition> DataDefinitions
        {
            get
            {
                if (_dataDefinitions == null)
                {
                    _dataDefinitions = new List<DataDefinition>();
                    if (_resource.ContainsKey("dataDefinitions"))
                    {
                        JsonObject dataDefinitions = (JsonObject) _resource.Get("dataDefinitions");
                        foreach (var key in dataDefinitions.Keys)
                        {
                            _dataDefinitions.Add(new DataDefinition(dataDefinitions[key]));
                        }
                    }
                }

                return _dataDefinitions;
            }
        }
    }
}