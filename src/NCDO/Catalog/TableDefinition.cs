using System.Collections.Generic;
using System.Json;
using NCDO.Extensions;

namespace NCDO.Catalog
{
    public class TableDefinition
    {
        private JsonValue _tableDefinition;
        private List<string> _primaryKey;
        private Dictionary<string, IPropertyDefinition> _properties;

        public TableDefinition(JsonValue tableDefinition)
        {
            _tableDefinition = tableDefinition;
        }

        public string Type => _tableDefinition.Get("type");

        public List<string> PrimaryKey
        {
            get
            {
                if (_primaryKey == null)
                {
                    _primaryKey = new List<string>();
                    var primaryKeyValue = _tableDefinition.Get("primaryKey");
                    if (primaryKeyValue.JsonType == JsonType.String)
                        _primaryKey.Add("ID");
                    else
                    {
                        foreach (var jsonValue in (JsonArray) primaryKeyValue)
                        {
                            _primaryKey.Add((string) jsonValue);
                        }
                    }
                }

                return _primaryKey;
            }
        }

        public Dictionary<string, IPropertyDefinition> Properties
        {
            get
            {
                if (_properties == null)
                {
                    _properties = new Dictionary<string, IPropertyDefinition>();
                    if (_tableDefinition.ContainsKey("items"))
                    {
                        JsonObject pdProperties = (JsonObject) _tableDefinition.Get("items")?.Get("properties");
                        foreach (var key in pdProperties.Keys)
                        {
                            _properties.Add(key, new PropertyDefinition(pdProperties[key]));
                        }
                    }
                }

                return _properties;
            }
        }
    }
}