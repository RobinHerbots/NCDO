using NCDO.Extensions;
using System;
using System.Collections.Generic;
using System.Json;
using System.Linq;
using System.Text;
using NCDO.Interfaces;

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
                Before = ds.Value.Get("prods:before") as JsonObject;
                HasChanges = ds.Value.Get("prods:hasChanges");
                ImportTables(ds.Value);
            }
        }

        protected internal virtual void ImportTables(JsonValue value)
        {
            //Import tables
            foreach (var key in ((JsonObject)value).Keys)
            {
                if (!key.StartsWith("prods:"))
                {
                    if (value.Get(key) is IEnumerable<JsonValue> tTable)
                        Join<CDO_Record>(key, new CDO_Table<CDO_Record>(tTable.Cast<JsonObject>()));
                }
            }
        }

        protected void Join<R>(string key, CDO_Table<R> value) where R : CDO_Record, new()
        {
            if (!ContainsKey(key)) Add(key, value);
            else
            {
                CDO_Table<R> table = (CDO_Table<R>)this[key];
                foreach (R record in value)
                {
                    if (!table.Contains(record))
                        table.Add(record);
                    else
                    {
                        var index = table.IndexOf(record);
                        var existing = table[index];
                        existing.Merge(record);
                    }
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
