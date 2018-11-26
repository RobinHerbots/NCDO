using System.Collections.Generic;
using System.Json;
using NCDO.Extensions;

namespace NCDO.Catalog
{
    public class DataDefinition
    {
        public DataDefinition(JsonValue dataDefinition)
        {
            Type = dataDefinition.Get("type");
            AdditionalProperties = dataDefinition.Get("additionalProperties");
            if (dataDefinition.ContainsKey("properties"))
            {
                Properties = new Dictionary<string, TableDefinition>();
                JsonObject pdProperties = (JsonObject)dataDefinition.Get("properties");
                foreach (var key in pdProperties.Keys)
                {
                    Properties.Add(key ,new TableDefinition(pdProperties[key]));
                }
            }
        }

        public string Type { get; }
        public bool AdditionalProperties { get; }
        public Dictionary<string, TableDefinition> Properties { get; }
    }
}
