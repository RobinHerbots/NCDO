using NCDO.Extensions;
using System;
using System.Collections.Generic;
using System.Json;
using System.Text;

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
                Properties = new List<IPropertyDefinition>();
                JsonObject ddProperties = (JsonObject)dataDefinition.Get("properties");
                foreach (var key in ddProperties.Keys)
                {
                    //Properties.Add(new DataDefinition(ddProperties[key]));
                }
            }
        }

        public string Type { get; }
        public bool AdditionalProperties { get; }
        public List<IPropertyDefinition> Properties { get; }
    }
}
