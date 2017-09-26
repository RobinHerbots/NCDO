using NCDO.Extensions;
using System;
using System.Collections.Generic;
using System.Json;
using System.Linq;
using System.Text;

namespace NCDO.CDOMemory
{
    /// <summary>
    /// CDO in memory implementation (Dataset)
    /// </summary>
    public class CDO_Dataset : JsonObject
    {

        #region Constructor
        public CDO_Dataset()
        {
        }
        public CDO_Dataset(IEnumerable<KeyValuePair<string, JsonValue>> items)
        {
            Init(items);
        } 
        #endregion

        internal void Init(IEnumerable<KeyValuePair<string, JsonValue>> items)
        {
            var ds = items.FirstOrDefault();
            if (!string.IsNullOrEmpty(ds.Key))
            {
                Name = ds.Key;
                Before = (JsonObject)ds.Value.Get("prods:before");
                HasChanges = (JsonPrimitive)ds.Value.Get("prods:hasChanges");
                ImportTables(ds.Value);
            }
        }

        public virtual void ImportTables(JsonValue value)
        {
            //Import tables
            foreach (var key in ((JsonObject)value).Keys)
            {
                if (!key.StartsWith("prods:"))
                {
                    Add(key, new CDO_Table(((IEnumerable<JsonValue>)  value.Get(key)).Cast<JsonObject>()));
                }
            }
        }


        /// <summary>
        /// Before state 
        /// </summary>
        public bool HasChanges { get; private set; }

        /// <summary>
        /// Before state 
        /// </summary>
        public JsonObject Before { get; private set; }

        /// <summary>
        /// Dataset name
        /// </summary>
        public String Name { get; private set; }
    }
}
