using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public abstract class CDO_Record<T> : CDO_Record where T : CDO_Record, new()
    {
        #region Constructor

        public CDO_Record(params JsonPair[] items): base(items)
        {
        }

        public CDO_Record(JsonPairEnumerable items): base(items)
        {
        }

        public CDO_Record()
        {
        }

        #endregion

        #region Overrides of CDO_Record

        /// <inheritdoc />
        public override string GetId()
        {
            if (!string.IsNullOrEmpty(PrimaryKeyName) && ContainsKey(PrimaryKeyName))
            {
                var id = this.Get(PrimaryKeyName).ToString();
                if (_defaultPrimaryKeyValue == id)
                {
                    return _id;
                }

                return id;
            }

            return base.GetId();
        }

        private string _defaultPrimaryKeyValue;

        /// <inheritdoc />
        public override string PrimaryKeyName
        {
            set
            {
                base.PrimaryKeyName = value;
                _defaultPrimaryKeyValue = Default(value)?.ToString();
            }
        }

        #endregion

        public virtual S Default<S>(Expression<Func<T, S>> propertyExpression)
        {
            var property = propertyExpression.Body as UnaryExpression;
            MemberExpression propExp =
                (property?.Operand as MemberExpression) ?? propertyExpression.Body as MemberExpression;
            var defaultValueAttribute = propExp?.Member.GetCustomAttribute<DefaultValueAttribute>();

            var converter = TypeDescriptor.GetConverter(typeof(S));
            return (S) converter.ConvertFromString(defaultValueAttribute?.Value.ToString());
        }

        protected virtual object Default(string propertyName)
        {
            var propInfo = GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
            var defaultValueAttribute = propInfo?.GetCustomAttribute<DefaultValueAttribute>();
            return defaultValueAttribute?.Value;
        }
    }


    public class CDO_Record : JsonObject, ICloudDataRecord, IEquatable<CDO_Record>
    {
        internal Dictionary<string, JsonValue> _changeDict = new Dictionary<string, JsonValue>();

        /// <summary>
        ///     An internal field for the CDO that is provided to find a given record in its memory.
        /// </summary>
        protected string _id = Guid.NewGuid().ToString();

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

        public bool IsPrimaryKeyChanged => !string.IsNullOrEmpty(PrimaryKeyName) && _changeDict.ContainsKey(PrimaryKeyName);
        public string GetChangedKeyValue => IsPrimaryKeyChanged ? _changeDict[PrimaryKeyName].AsString() : GetId();

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
        public virtual string GetId() => string.IsNullOrEmpty(PrimaryKeyName) || !ContainsKey(PrimaryKeyName)
            ? _id
            : this.Get(PrimaryKeyName).ToString();

        public virtual void SetId(JsonValue value)
        {
            if (string.IsNullOrEmpty(PrimaryKeyName))
                _id = value.AsString();
            else this.Set(PrimaryKeyName, value);
        }

        public virtual string PrimaryKeyName { get; set; }

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
            {
                Add(keyValuePair.Key, keyValuePair.Value);
            }
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
        
        #region Status

        public bool IsNew => !Int32.TryParse(GetId(), out int id) || id <= 0;

        #endregion
    }
}