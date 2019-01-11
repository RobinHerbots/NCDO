using System.Collections.Generic;
using System.Json;
using NCDO.Extensions;

namespace NCDO.Catalog
{
    public class Schema
    {
        private readonly JsonValue _schema;
        private Dictionary<string, DataDefinition> _properties;

        public Schema(JsonValue schema)
        {
            _schema = schema;
        }

        public string Type => _schema.Get("type");
        public bool AdditionalProperties => _schema.Get("additionalProperties");

        public Dictionary<string, DataDefinition> Properties
        {
            get
            {
                if (_properties == null)
                {
                    _properties = new Dictionary<string, DataDefinition>();
                    if (_schema.ContainsKey("properties"))
                    {
                        JsonObject schemaProperties = (JsonObject) _schema.Get("properties");
                        foreach (var key in schemaProperties.Keys)
                        {
                            _properties.Add(key, new DataDefinition(schemaProperties[key]));
                        }
                    }
                }

                return _properties;
            }
        }
    }
}