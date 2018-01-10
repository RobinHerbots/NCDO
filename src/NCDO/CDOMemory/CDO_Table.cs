using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Json;
using System.Linq;
using System.Reflection;
using NCDO.Definitions;
using NCDO.Extensions;
using NCDO.Interfaces;
using DataRowState = NCDO.Definitions.DataRowState;

namespace NCDO.CDOMemory
{
    public class CDO_Table<T> : JsonArray, IList<T>, INotifyCollectionChanged, IChangeTracking
        where T : CDO_Record, new()
    {
        protected List<T> _list;
        internal List<T> _new = new List<T>();
        internal List<T> _changed = new List<T>();
        internal List<T> _deleted = new List<T>();

        public CDO_Table(params T[] items)
        {
            _list = new List<T>();
            AddRange(items, MergeMode.Append, false);
        }

        public CDO_Table(IEnumerable<T> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            _list = new List<T>();
            AddRange(items, MergeMode.Append, false);
        }

        public CDO_Table(IEnumerable<JsonObject> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            _list = new List<T>();
            AddRange(items.Select(i =>
            {
                var record = new T();
                record.AddRange(i);
                return record;
            }), MergeMode.Append, false);
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
        /// <param name="notify"></param>
        public void Add(T item, MergeMode mergeMode, bool notify = true)
        {
            if (!Contains(item))
            {
                _list.Add(item);
                item.PropertyChanged -= Item_PropertyChanged;
                item.PropertyChanged += Item_PropertyChanged;
                if (notify)
                    OnCollectionChanged(NotifyCollectionChangedAction.Add, new[] { item });
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
                        if (notify)
                            OnCollectionChanged(NotifyCollectionChangedAction.Replace, new[] { this[index] }, index);
                        break;
                    case MergeMode.Replace:
                        RemoveAt(index);
                        _list.Add(item);
                        if (notify)
                            OnCollectionChanged(NotifyCollectionChangedAction.Replace, new[] { item }, index);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(mergeMode), mergeMode, null);
                }
            }
        }

        /// <summary>
        /// Detect changes from childrecords
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnCollectionChanged(sender, e);
        }

        public new void Clear()
        {
            var oldList = new List<T>();
            oldList.AddRange(_list);
            _list.Clear();
            OnCollectionChanged(NotifyCollectionChangedAction.Reset, oldList);
        }

        public bool Contains(T item) => Contains(_list, item);

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
                var ndx = IndexOf(item);
                _list.RemoveAt(ndx);
                OnCollectionChanged(NotifyCollectionChangedAction.Move, new[] { item });
            }
            else
            {
                OnCollectionChanged(NotifyCollectionChangedAction.Add, new[] { item });
            }
            _list.Insert(index, item);
        }

        public bool IsReadOnly => false;

        public new T this[int index]
        {
            get => _list[index];
            set { Insert(index, value); }
        }

        public bool Remove(T item)
        {
            var index = IndexOf(item);
            RemoveAt(index);
            return true;
        }

        public void RemoveAt(int index)
        {
            OnCollectionChanged(NotifyCollectionChangedAction.Remove, null, index);
            _list.RemoveAt(index);
        }

        public void AddRange(IEnumerable<T> items)
        {
            AddRange(items, MergeMode.Append);
        }

        public void AddRange(IEnumerable<T> items, MergeMode mergeMode, bool notify = true)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            foreach (var item in items)
            {
                Add(item, mergeMode, notify);
            }
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

        protected virtual void OnCollectionChanged(NotifyCollectionChangedAction action, IEnumerable<T> items = null,
            int index = -1)
        {
            switch (action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddNew(_new, items, DataRowState.Created);
                    break;
                case NotifyCollectionChangedAction.Move:
                    if (items != null)
                        foreach (var item in items)
                        {
                            if (_new.All(r => r.GetId() != item.GetId()))
                                AddNew(_changed, new[] { item }, DataRowState.Modified);
                        }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (index != -1)
                        AddNew(_deleted, new[] { _list[index] }, DataRowState.Deleted);
                    AddNew(_deleted, items, DataRowState.Deleted);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    AddNew(_changed, items, DataRowState.Modified);
                    AddNew(_deleted, new[] { _list[index] }, DataRowState.Deleted);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    AddNew(_deleted, items, DataRowState.Deleted);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }


            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, items, index));
        }

        private void OnCollectionChanged(object sender, PropertyChangedEventArgs e)
        {
            OnCollectionChanged(NotifyCollectionChangedAction.Move, new[] { (T)sender });
        }

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
                foreach (var propertyInfo in target.GetType()
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
                {
                    if (propertyInfo.CanRead && propertyInfo.CanWrite && !target.IsPropertyChanged(propertyInfo.Name))
                    {
                        var sourceValue = propertyInfo.GetValue(source);
                        propertyInfo.SetValue(target, sourceValue);
                    }
                }
            }
        }

        private void AddNew(IList<T> list, IEnumerable<T> items, Definitions.DataRowState rowState)
        {
            if (items != null)
            {
                foreach (var item in items)
                {
                    if (!Contains(list, item))
                    {
                        if (rowState == DataRowState.Modified)
                            item.Set("prods:id", item.GetId());
                        item.Set("prods:rowState", rowState.ToString().ToLowerInvariant());
                        item.Set("prods:clientId", item.GetId());
                        list.Add(item);
                    }
                }
            }
        }

        private bool Contains(IList<T> list, T item)
        {
            var itemId = item.GetId();
            return list.Any(i => i.GetId() == itemId);
        }

        #endregion

        #region Implementation of IChangeTracking

        /// <inheritdoc />
        public void AcceptChanges()
        {
            _new.Clear();
            _changed.Clear();
            _deleted.Clear();
        }

        /// <inheritdoc />
        public bool IsChanged => _changed.Count > 0 || _deleted.Count > 0;

        #endregion

        public void RejectChanges()
        {
            foreach (var record in _new)
            {
                var index = IndexOf(record);
                _list.RemoveAt(index);
            }
            foreach (var record in _changed)
            {
                record.RejectRowChanges();
            }
            _changed.Clear();
            AddRange(_deleted);
            _deleted.Clear();
        }
    }
}