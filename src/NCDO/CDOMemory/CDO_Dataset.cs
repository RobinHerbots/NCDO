using System;
using System.Collections.Generic;
using System.Json;
using System.Linq;
using NCDO.Definitions;
using NCDO.Extensions;

namespace NCDO.CDOMemory
{
    /// <summary>
    /// CDO in memory implementation (Dataset)
    /// </summary>
    public partial class CDO_Dataset : JsonObject
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

        internal void Init(IEnumerable<KeyValuePair<string, JsonValue>> items = null)
        {
            var ds = items?.FirstOrDefault();
            if (!string.IsNullOrEmpty(ds?.Key))
            {
                Name = ds?.Key;
                Before = ds?.Value.Get("prods:before") as JsonObject;
                HasChanges = ds?.Value.Get("prods:hasChanges");
            }
            ImportTables(ds?.Value);
        }

        protected internal virtual void ImportTables(JsonValue value)
        {
            //Import tables
            if (value != null)
            {
                foreach (var key in ((JsonObject)value).Keys)
                {
                    if (!key.StartsWith("prods:"))
                    {
                        if (value.Get(key) is IEnumerable<JsonValue> tTable)
                            Add<CDO_Record>(key, new CDO_Table<CDO_Record>(tTable.Cast<JsonObject>()));
                        else Add<CDO_Record>(key, new CDO_Table<CDO_Record>());
                    }
                }
            }
        }

        protected void Add<R>(string key, CDO_Table<R> value) where R : CDO_Record, new()
        {
            if (!ContainsKey(key)) base.Add(key, value);
            else
            {
                var table = (CDO_Table<R>)this[key];
                table.AddRange(value, MergeMode.Replace, false);
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
