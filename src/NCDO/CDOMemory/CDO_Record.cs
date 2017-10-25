// Licensed to the .NET Foundation under one or more agreements.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Json;
using System.Reflection;
using System.Text;
using NCDO.Extensions;
using NCDO.Interfaces;
using JsonPair = System.Collections.Generic.KeyValuePair<string, System.Json.JsonValue>;
using JsonPairEnumerable = System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, System.Json.JsonValue>>;
using System.ComponentModel.DataAnnotations;

namespace NCDO.CDOMemory
{
    public class CDO_Record : JsonObject, ICloudDataRecord
    {
        #region Constructor

        public CDO_Record(params JsonPair[] items) : base(items)
        {
            SetPrimaryKey();
        }

        public CDO_Record(JsonPairEnumerable items) : base(items)
        {
            SetPrimaryKey();
        }

        public CDO_Record() : base()
        {
            SetPrimaryKey();
        }

        #endregion

        #region Internal Properties

        protected internal string primaryKey;

        /// <summary>
        /// An internal field for the CDO that is provided to find a given record in its memory.
        /// </summary>
        private string _id = Guid.NewGuid().ToString();

        /// <summary>
        /// Used by the CDO to do automatic data mapping for any error string passed back from backend with before-imaging data
        /// </summary>
        private string _errorString;

        #endregion

        private void SetPrimaryKey()
        {
            if (string.IsNullOrEmpty(primaryKey))
            {
                var props = this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
                foreach (var propertyInfo in props)
                {
                    if (propertyInfo.GetCustomAttribute<KeyAttribute>() != null)
                    {
                        primaryKey = propertyInfo.Name;
                        break;
                    }
                }
            }
        }

        #region Implementation of ICloudDataRecord

        /// <inheritdoc />
        public JsonPairEnumerable Data => this;

        /// <inheritdoc />
        public void AcceptRowChanges()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Assign()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public string GetErrorString()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual string GetId()
        {
            if (string.IsNullOrEmpty(primaryKey))
            {
                return _id;
            }
            var pkValue = this.Get(primaryKey).ToString();
            return string.IsNullOrEmpty(pkValue) ? _id : pkValue;
        }

        /// <inheritdoc />
        public void Remove()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void RejectRowChanges()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}