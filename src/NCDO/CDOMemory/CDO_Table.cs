using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Json;
using System.Linq;
using System.Reflection;
using NCDO.Definitions;
using NCDO.Interfaces;

namespace NCDO.CDOMemory
{
    public class CDO_Table<T> : JsonArray, IList<T>, INotifyCollectionChanged where T : CDO_Record, new()
    {
        protected List<T> _list;

        public CDO_Table(params T[] items)
        {
            _list = new List<T>();
            AddRange(items);
        }

        public CDO_Table(IEnumerable<T> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            _list = new List<T>(items);
        }

        public CDO_Table(IEnumerable<JsonObject> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            _list = new List<T>(items.Select(i =>
            {
                var record = new T();
                record.AddRange(i);
                return record;
            }));
        }

        public override JsonType JsonType => JsonType.Array;

        public void Add(T item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            Add(item, MergeMode.Append);
        }

        /// <summary>
        /// Add a new record or merge an existing
        /// </summary>
        /// <param name="item"></param>
        /// <param name="mergeMode"></param>
        public void Add(T item, MergeMode mergeMode)
        {
            if (!Contains(item))
            {
                _list.Add(item);
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new[] { item }));
            }
            else
            {
                var index = IndexOf(item);
                switch (mergeMode)
                {
                    case MergeMode.Empty:
                        break;
                    case MergeMode.Append:
                        throw new CDOException($"Duplicate record with ID {item.GetId()}");
                    case MergeMode.Merge:
                        Merge(this[index], item);
                        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, new[] { this[index] }, index));
                        break;
                    case MergeMode.Replace:
                        RemoveAt(index);
                        _list.Add(item);
                        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, new[] { item }, index));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(mergeMode), mergeMode, null);
                }
            }
        }


        public new void Clear()
        {
            _list.Clear();
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public bool Contains(T item)
        {
            var itemId = item.GetId();
            return _list.Any(i => i.GetId() == itemId);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public override int Count => _list.Count;

        public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

        public int IndexOf(T item)
        {
            var itemId = item.GetId();
            return _list.FindIndex(i => i.GetId() == itemId);
        }

        public void Insert(int index, T item)
        {
            if (Contains(item))
            {
                _list.Remove(item);
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, new[] { item }, index));
            }
            else
            {
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new[] { item }, index));
            }
            _list.Insert(index, item);
        }

        public bool IsReadOnly => false;

        public new T this[int index]
        {
            get => _list[index];
            set => _list[index] = value;
        }

        public bool Remove(T item)
        {
            var ret = _list.Remove(item);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new[] { item }));
            return ret;
        }

        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, null, index));
        }

        public void AddRange(IEnumerable<T> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            _list.AddRange(items);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, null, items));
        }

        public void AddRange(params T[] items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            _list.AddRange(items);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, null, items));
        }

        public override void Save(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            stream.WriteByte((byte)'[');

            for (var i = 0; i < _list.Count; i++)
            {
                JsonValue v = _list[i];
                if (v != null)
                {
                    v.Save(stream);
                }
                else
                {
                    stream.WriteByte((byte)'n');
                    stream.WriteByte((byte)'u');
                    stream.WriteByte((byte)'l');
                    stream.WriteByte((byte)'l');
                }

                if (i < Count - 1)
                {
                    stream.WriteByte((byte)',');
                    stream.WriteByte((byte)' ');
                }
            }

            stream.WriteByte((byte)']');
        }

        #region Implementation of INotifyCollectionChanged

        /// <inheritdoc />
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        #endregion

        #region private
        /// <summary>
        /// merge record into the target record.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        private void Merge<T>(T target, T source) where T : ICloudDataRecord
        {
            if (source != null)
            {
                foreach (var propertyInfo in target.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                )
                {
                    if (propertyInfo.CanRead)
                    {
                        var targetValue = propertyInfo.GetValue(target);
                        if (propertyInfo.CanWrite && DefaultValue(propertyInfo.PropertyType) == targetValue)
                        {
                            propertyInfo.SetValue(target, propertyInfo.GetValue(source));
                        }
                    }
                }
            }
        }

        private object DefaultValue(Type targetType)
        {
            return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
        }
        #endregion
    }
}