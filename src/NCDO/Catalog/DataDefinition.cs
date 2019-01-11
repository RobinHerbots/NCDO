using System.Collections.Generic;
using System.Json;
using NCDO.Extensions;

namespace NCDO.Catalog
{
    public class DataDefinition
    {
        private JsonValue _dataDefinition;
        private Dictionary<string, TableDefinition> _properties;

        public DataDefinition(JsonValue dataDefinition)
        {
            _dataDefinition = dataDefinition;
        }

        public string Type => _dataDefinition.Get("type");
        public bool AdditionalProperties => _dataDefinition.Get("additionalProperties");

        public Dictionary<string, TableDefinition> Properties
        {
            get
            {
                if (_properties == null)
                {
                    _properties = new Dictionary<string, TableDefinition>();
                    if (_dataDefinition.ContainsKey("properties"))
                    {
                        JsonObject pdProperties = (JsonObject) _dataDefinition.Get("properties");
                        foreach (var key in pdProperties.Keys)
                        {
                            _properties.Add(key, new TableDefinition(pdProperties[key]));
                        }
                    }
                }

                return _properties;
            }
        }
    }
}