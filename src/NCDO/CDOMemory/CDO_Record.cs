// Licensed to the .NET Foundation under one or more agreements.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Json;
using System.Reflection;
using System.Runtime.CompilerServices;
using NCDO.Extensions;
using NCDO.Interfaces;
using JsonPair = System.Collections.Generic.KeyValuePair<string, System.Json.JsonValue>;
using JsonPairEnumerable =
    System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, System.Json.JsonValue>>;

namespace NCDO.CDOMemory
{
    public class CDO_Record : JsonObject, ICloudDataRecord
    {
        internal Dictionary<string, JsonValue> _changeDict = new Dictionary<string, JsonValue>();
        /// <summary>
        ///     An internal field for the CDO that is provided to find a given record in its memory.
        /// </summary>
        private readonly string _id = Guid.NewGuid().ToString();

        /// <summary>
        ///     Used by the CDO to do automatic data mapping for any error string passed back from backend with before-imaging data
        /// </summary>
        private string _errorString;

        protected internal string primaryKey;

        private void SetPrimaryKey()
        {
            if (string.IsNullOrEmpty(primaryKey))
            {
                var props = GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
                foreach (var propertyInfo in props)
                    if (propertyInfo.GetCustomAttribute<KeyAttribute>() != null)
                    {
                        primaryKey = propertyInfo.Name;
                        break;
                    }
            }
        }

        #region Constructor

        public CDO_Record(params JsonPair[] items) : base(items)
        {
            SetPrimaryKey();
        }

        public CDO_Record(JsonPairEnumerable items) : base(items)
        {
            SetPrimaryKey();
        }

        public CDO_Record()
        {
            SetPrimaryKey();
        }

        #endregion
        #region Implementation of ICloudDataRecord

        /// <inheritdoc />
        public JsonPairEnumerable Data => this;

        /// <inheritdoc />
        public void AcceptRowChanges()
        {
            _changeDict.Clear();
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
                return _id;
            var pkValue = this.Get(primaryKey).ToString().Trim('"');
            return string.IsNullOrEmpty(pkValue) ? _id : pkValue;
        }

        /// <inheritdoc />
        public void RejectRowChanges()
        {
            foreach (var keyValuePair in _changeDict)
            {
                this[keyValuePair.Key] = keyValuePair.Value;
            }
            _changeDict.Clear();
        }

        /// <inheritdoc />
        public bool IsPropertyChanged(string propertyName) => _changeDict.ContainsKey(propertyName);

        #endregion

        #region Implementation of INotifyPropertyChang(ing|ed)
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <inheritdoc />
        public event PropertyChangingEventHandler PropertyChanging;
        protected virtual void OnPropertyChanging(string propertyName)
        {
            PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
            if (!_changeDict.ContainsKey(propertyName)) _changeDict.Add(propertyName, this[propertyName]);
        }

        public new void Add(string key, JsonValue value)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (ContainsKey(key)) OnPropertyChanging(key);
            base.Add(key, value);
            if (IsPropertyChanged(key)) OnPropertyChanged(key);
        }

        public new void AddRange(IEnumerable<KeyValuePair<string, JsonValue>> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            foreach (KeyValuePair<string, JsonValue> keyValuePair in items)
                Add(keyValuePair.Key, keyValuePair.Value);
        }

        public new JsonValue this[string key]
        {
            get => base[key];
            set
            {
                if (ContainsKey(key)) OnPropertyChanging(key);
                base[key] = value;
                if (IsPropertyChanged(key)) OnPropertyChanged(key);
            }
        }

        #endregion

        #region Implementation of IChangeTracking

        /// <inheritdoc />
        public void AcceptChanges()
        {
            AcceptRowChanges();
        }

        /// <inheritdoc />
        public bool IsChanged => _changeDict.Count > 0;
        #endregion
    }
}