// Licensed to the .NET Foundation under one or more agreements.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Json;
using System.Linq.Expressions;
using System.Reflection;
using NCDO.Extensions;
using NCDO.Interfaces;
using JsonPair = System.Collections.Generic.KeyValuePair<string, System.Json.JsonValue>;
using JsonPairEnumerable =
    System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, System.Json.JsonValue>>;

namespace NCDO.CDOMemory
{
    public partial class CDO_Record<T> : CDO_Record where T : CDO_Record, new()
    {
        protected static JsonObject Defaults = new T();
        private static string _primaryKey = "";

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
            InitializeRecord(); //only initialize after _defaults instantiation
            primaryKey = _primaryKey;
            if (!string.IsNullOrEmpty(primaryKey))
                _pkValue = Defaults?.Get(primaryKey);
        }

        #endregion


        private void InitializeRecord()
        {
            if (Defaults == null) return;
            lock (Defaults)
            {
                if ((Defaults).Count == 0)
                {
                    var props = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);
                    foreach (var propertyInfo in props)
                    {
                        var defaultValueAttribute = propertyInfo.GetCustomAttribute<DefaultValueAttribute>();
                        if (defaultValueAttribute != null && propertyInfo.CanWrite)
                        {
                            var targetType = propertyInfo.PropertyType.IsNullableType()
                                ? Nullable.GetUnderlyingType(propertyInfo.PropertyType)
                                : propertyInfo.PropertyType;
                            propertyInfo.SetValue(Defaults,
                                defaultValueAttribute.Value != null
                                    ? Convert.ChangeType(defaultValueAttribute.Value, targetType)
                                    : defaultValueAttribute.Value);
                        }

                        if (string.IsNullOrEmpty(_primaryKey) &&
                            propertyInfo.GetCustomAttribute<KeyAttribute>() != null)
                        {
                            _primaryKey = propertyInfo.Name;
                        }
                    }
                }
            }

            foreach (var keyValuePair in Defaults)
            {
                Add(keyValuePair.Key, keyValuePair.Value, false);
            }
        }

        public virtual S Default<S>(Expression<Func<T, S>> propertyExpression)
        {
            var property = propertyExpression.Body as UnaryExpression;
            MemberExpression propExp =
                (property?.Operand as MemberExpression) ?? propertyExpression.Body as MemberExpression;
            var defaultValueAttribute = propExp?.Member.GetCustomAttribute<DefaultValueAttribute>();

            var converter = TypeDescriptor.GetConverter(typeof(S));
            return (S) converter.ConvertFromString(defaultValueAttribute?.Value.ToString());
        }

        #region Overrides of CDO_Record

        private object _pkValue = null; //default PK value ~ performance CRUD

        /// <inheritdoc />
        public override string GetId()
        {
            return string.IsNullOrEmpty(primaryKey) ? _id : this.Get(primaryKey).AsString();
        }

        #endregion


        public override ICollection<string> Keys
        {
            get
            {
                lock (Defaults)
                {
                    return Defaults != null ? Defaults.Keys : base.Keys;
                }
            }
        }
    }


    public class CDO_Record : JsonObject, ICloudDataRecord, IEquatable<CDO_Record>
    {
        internal Dictionary<string, JsonValue> _changeDict = new Dictionary<string, JsonValue>();

        /// <summary>
        ///     An internal field for the CDO that is provided to find a given record in its memory.
        /// </summary>
        protected readonly string _id = Guid.NewGuid().ToString();

        /// <summary>
        /// this field is used in the generic version of CDO_Record
        /// </summary>
        protected internal string primaryKey = "";

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

        //redefine is needed to allow override by generic CDO_Record
        public new virtual ICollection<string> Keys => base.Keys;

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
                OnPropertyChanging(key);
                base.Remove(key);
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
            return Equals((CDO_Record) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode() => (GetId() != null ? GetId().GetHashCode() : 0);

        #endregion
    }
}