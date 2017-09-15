using NCDO.Extensions;
using System;
using System.Collections.Generic;
using System.Json;
using System.Linq;
using System.Text;

namespace NCDO
{
    public class CDOMemory : JsonObject
    {

        public CDOMemory(IEnumerable<KeyValuePair<string, JsonValue>> items)
        {
            var ds = items.FirstOrDefault();
            if (!string.IsNullOrEmpty(ds.Key))
            {
                Name = ds.Key;
                Before = (JsonObject)ds.Value.Get("prods:before");
                HasChanges = (JsonPrimitive)ds.Value.Get("prods:hasChanges");
                //Import tables
                foreach(string key in ((JsonObject)ds.Value).Keys)
                {
                    if (!key.StartsWith("prods:"))
                    {
                        Add(key,ds.Value.Get(key));
                    }
                }
            }
        }

        /// <summary>
        /// Before state 
        /// </summary>
        public bool HasChanges { get; }

        /// <summary>
        /// Before state 
        /// </summary>
        public JsonObject Before { get; }

        /// <summary>
        /// Dataset name
        /// </summary>
        public String Name { get; }

    }
}
