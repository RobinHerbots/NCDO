using System.Collections.Generic;
using System.Json;
using NCDO.Extensions;

namespace NCDO.Catalog
{
    public class TableDefinition
    {
        public TableDefinition(JsonValue tableDefinition)
        {
            Type = tableDefinition.Get("type");
            var primaryKeyValue = tableDefinition.Get("primaryKey");
            if (primaryKeyValue.JsonType == JsonType.String)
                PrimaryKey.Add("ID");
            else
            {
                foreach (var jsonValue in (JsonArray)primaryKeyValue)
                {
                    PrimaryKey.Add((string)jsonValue);
                }
            }
            if (tableDefinition.ContainsKey("items"))
            {
                Properties = new Dictionary<string, IPropertyDefinition>();
                JsonObject pdProperties = (JsonObject)tableDefinition.Get("items")?.Get("properties");
                foreach (var key in pdProperties.Keys)
                {
                    Properties.Add(key, new PropertyDefinition(pdProperties[key]));
                }
            }
        }

        public string Type { get; }
        public List<string> PrimaryKey { get; } = new List<string>();
        public Dictionary<string, IPropertyDefinition> Properties { get; }
    }
}
