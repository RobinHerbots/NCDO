using NCDO.Extensions;
using System;
using System.Collections.Generic;
using System.Json;
using System.Text;

namespace NCDO.Catalog
{
    public class Schema
    {
        public Schema(JsonValue schema)
        {
            Type = schema.Get("type");
            AdditionalProperties = schema.Get("additionalProperties");
            if (schema.ContainsKey("properties"))
            {
                Properties = new Dictionary<string, DataDefinition>();
                JsonObject schemaProperties = (JsonObject)schema.Get("properties");
                foreach (var key in schemaProperties.Keys)
                {
                    Properties.Add(key, new DataDefinition(schemaProperties[key]));
                }
            }
        }

        public string Type { get; }
        public bool AdditionalProperties { get; }
        public Dictionary<string, DataDefinition> Properties { get; }
    }
}
