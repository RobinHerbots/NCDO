using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Json;
using System.Linq;
using System.Threading.Tasks;
using NCDO.Definitions;
using NCDO.Extensions;
using NCDO.Interfaces;

namespace NCDO.CDOMemory
{
    /// <summary>
    /// CDO in memory implementation (Dataset)
    /// </summary>
    public partial class CDO_Dataset : JsonObject, INormalize, IValidatableObject
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

        protected virtual void ImportTables(JsonValue value)
        {
            //Import tables
            if (value != null)
            {
                var taskList = new List<Task>();
                foreach (var key in ((JsonObject) value).Keys)
                {
                    if (!key.StartsWith("prods:"))
                    {
                        taskList.Add(Task.Factory.StartNew(() =>
                        {
                            if (value.Get(key) is IEnumerable<JsonValue> tTable)
                                Add<CDO_Record>(key, new CDO_Table<CDO_Record>(tTable.Cast<JsonObject>()));
                            else Add<CDO_Record>(key, new CDO_Table<CDO_Record>());
                        }));
                    }
                }

                Task.WaitAll(taskList.ToArray());
            }
        }

        protected void Add<R>(string key, CDO_Table<R> value) where R : CDO_Record, new()
        {
            if (!ContainsKey(key)) base.Add(key, value);
            else
            {
                var table = (CDO_Table<R>) this[key];
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

        #region Implementation of INormalize

        /// <inheritdoc />
        public void Normalize()
        {
            foreach (var key in Keys)
            {
                if (this.Get(key) is INormalize normalize)
                    normalize.Normalize();
            }
        }

        #endregion

        #region Implementation of IValidatableObject

        /// <inheritdoc />
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            foreach (var key in Keys)
            {
                if (this.Get(key) is IValidatableObject validatableObject)
                    Validator.TryValidateObject(validatableObject, new ValidationContext(validatableObject, null, null),
                        results);
            }

            return results;
        }

        #endregion
    }
}