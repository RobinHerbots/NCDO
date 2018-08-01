// Licensed to the .NET Foundation under one or more agreements.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Json;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using NCDO.Extensions;
using NCDO.Interfaces;
using JsonPair = System.Collections.Generic.KeyValuePair<string, System.Json.JsonValue>;
using JsonPairEnumerable = System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, System.Json.JsonValue>>;

namespace NCDO.CDOMemory
{
    public partial class CDO_Record<T> : CDO_Record where T : CDO_Record, new()
    {
        #region Constructor
        public CDO_Record(params JsonPair[] items) : this()
        {
            AddRange(items);
        }

        public CDO_Record(JsonPairEnumerable items) : this()
        {
            AddRange(items);
        }

        public CDO_Record()
        {
            if (_defaults != null) InitializeRecord(); //only initialize after _defaults instantiation
        }

        #endregion

        protected static T _defaults = new T();
        private void InitializeRecord()
        {
            lock (_defaults)
            {
                if (_defaults.Count == 0)
                {
                    var props = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);
                    foreach (var propertyInfo in props)
                    {
                        if (string.IsNullOrEmpty(primaryKey) &&
                            propertyInfo.GetCustomAttribute<KeyAttribute>() != null)
                        {
                            _defaults.primaryKey = propertyInfo.Name;
                        }

                        var defaultValueAttribute = propertyInfo.GetCustomAttribute<DefaultValueAttribute>();
                        if (defaultValueAttribute != null && propertyInfo.CanWrite)
                        {
                            var targetType = propertyInfo.PropertyType.IsNullableType()
                                ? Nullable.GetUnderlyingType(propertyInfo.PropertyType)
                                : propertyInfo.PropertyType;
                            propertyInfo.SetValue(_defaults,
                            defaultValueAttribute.Value != null
                                ? Convert.ChangeType(defaultValueAttribute.Value, targetType)
                                : defaultValueAttribute.Value);
                        }
                    }
                }
            }

            primaryKey = _defaults.primaryKey;
            foreach (var keyValuePair in _defaults) { Add(keyValuePair.Key, keyValuePair.Value, false); }
        }
        public virtual S Default<S>(Expression<Func<T, S>> propertyExpression)
        {
            var property = propertyExpression.Body as UnaryExpression;
            MemberExpression propExp = (property?.Operand as MemberExpression) ?? propertyExpression.Body as MemberExpression;
            var defaultValueAttribute = propExp?.Member.GetCustomAttribute<DefaultValueAttribute>();

            var converter = TypeDescriptor.GetConverter(typeof(S));
            return (S)converter.ConvertFromString(defaultValueAttribute?.Value.ToString());
        }

        #region Overrides of CDO_Record
        /// <inheritdoc />
        public override string GetId()
        {
            if (string.IsNullOrEmpty(primaryKey))
                return _id;
            var pkValue = this.Get(primaryKey).ToString().Trim('"');
            var defaultValueAttribute = this.GetType().GetProperty(primaryKey, BindingFlags.Public | BindingFlags.Instance)?.GetCustomAttribute<DefaultValueAttribute>();
            return defaultValueAttribute != null
                ? (defaultValueAttribute.Value.ToString() == pkValue ? _id : pkValue)
                : (string.IsNullOrEmpty(pkValue) ? _id : pkValue);
        }

        #endregion
    }


    public class CDO_Record : JsonObject, ICloudDataRecord, IEquatable<CDO_Record>
    {
        internal Dictionary<string, JsonValue> _changeDict = new Dictionary<string, JsonValue>();
        /// <summary>
        ///     An internal field for the CDO that is provided to find a given record in its memory.
        /// </summary>
        protected readonly string _id = Guid.NewGuid().ToString();

        /// <summary>
        ///     Used by the CDO to do automatic data mapping for any error string passed back from backend with before-imaging data
        /// </summary>
        private string _errorString;
        /// <summary>
        /// this field is used in the generic version of CDO_Record
        /// </summary>
        protected internal string primaryKey;

        #region Constructor
        public CDO_Record(params JsonPair[] items) : base(items)
        {
        }

        public CDO_Record(JsonPairEnumerable items) : base(items)
        {
        }

        public CDO_Record()
        {
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
        public void Assign(IEnumerable<KeyValuePair<string, JsonValue>> values)
        {
            this.AddRange(values);
        }

        /// <inheritdoc />
        public string GetErrorString()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual string GetId() => _id;

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

        public void Add(string key, JsonValue value, bool notify = true)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            if (notify && ContainsKey(key))
            {
                OnPropertyChanging(key); base.Remove(key);
            }
            base[key] = value;
            if (notify && IsPropertyChanged(key)) OnPropertyChanged(key);
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

        #region Equality members

        /// <inheritdoc />
        public virtual bool Equals(CDO_Record other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(GetId(), other.GetId());
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CDO_Record)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode() => (GetId() != null ? GetId().GetHashCode() : 0);

        #endregion
    }
}