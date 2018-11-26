using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Json;
using System.Linq;
using NCDO.Definitions;
using NCDO.Extensions;
using NCDO.Interfaces;

namespace NCDO.CDOMemory
{
    public partial class CDO_Table<T> : JsonArray, IList<T>, INotifyCollectionChanged, IChangeTracking
        where T : CDO_Record, new()
    {
        protected List<T> _list;
        internal List<T> _new = new List<T>();
        internal List<T> _changed = new List<T>();
        internal List<T> _deleted = new List<T>();

        public CDO_Table(params T[] items) : this((IEnumerable<T>)items)
        { }

        public CDO_Table(IEnumerable<T> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            _list = items.ToList();
            _list.ForEach(item => item.PropertyChanged += Item_PropertyChanged);
        }

        public CDO_Table(IEnumerable<JsonObject> items) : this(items.Select(i =>
        {
            var record = new T();
            foreach (var keyValuePair in i) { record.Add(keyValuePair.Key, keyValuePair.Value, false); }
            return record;
        }))
        { }

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
            _internalAdd(item, mergeMode, notify);
        }
        /// <summary>
        /// Internal Add fn
        /// justAdd to skip contains check ~ performance
        /// </summary>
        /// <param name="item"></param>
        /// <param name="mergeMode"></param>
        /// <param name="notify"></param>
        /// <param name="justAdd"></param>
        private void _internalAdd(T item, MergeMode mergeMode, bool notify = true, bool justAdd = false)
        {
            lock (_list)
            {
                if (justAdd || !_list.Contains(item))
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
                            if (Merge(this[index], item) && notify)
                                OnCollectionChanged(NotifyCollectionChangedAction.Move, new[] { this[index] });
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
        }

        /// <summary>
        /// Detect changes from childrecords
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
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

        public bool Contains(T item) => _list.Contains(item);

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
            if (_list.Contains(item))
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

        public new bool IsReadOnly => false;

        public new T this[int index]
        {
            get => _list[index];
            set => Insert(index, value);
        }

        public bool Remove(T item)
        {
            var index = IndexOf(item);
            RemoveAt(index);
            return true;
        }

        public new void RemoveAt(int index)
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

            var emptyList = _list.Count == 0;
            foreach (var item in items)
            {
                _internalAdd(item, mergeMode, notify, emptyList);
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
                            lock (_new)
                            {
                                if (_new.All(r => r.GetId() != item.GetId()))
                                {
                                    AddNew(_changed, new[] { item }, DataRowState.Modified);
                                }
                            }
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
        internal bool Merge<S>(S target, JsonObject source) where S : CDO_Record, ICloudDataRecord
        {
            var changed = false;
            if (source != null)
            {
                foreach (var key in target.Keys.ToList())
                {
                    if (!target.IsPropertyChanged(key))
                    {
                        target.Set(key, source.Get(key));
                        changed = true;
                    }
                }
            }

            return changed;
        }

        private void AddNew(IList<T> list, IEnumerable<T> items, DataRowState rowState)
        {
            if (items != null)
            {
                foreach (var item in items)
                {
                    lock (list)
                    {
                        if (!list.Contains(item))
                        {
                            var itemClone = item;
                            if (rowState == DataRowState.Deleted)
                            {
                                itemClone = new T();
                                itemClone.AddRange(item.Data);
                            }

                            if (rowState == DataRowState.Modified)
                                itemClone.Set("prods:id", item.GetId());
                            itemClone.Set("prods:rowState", rowState.ToString().ToLowerInvariant());
                            itemClone.Set("prods:clientId", item.GetId()); //defines the id used on the client
                            list.Add(itemClone);
                        }
                    }
                }
            }
        }

        private bool Contains(IList<T> list, T item)
        {
            lock (list)
            {
                var itemId = item.GetId();
                return list.Any(i => i.GetId() == itemId);
            }
        }

        #endregion

        #region Implementation of IChangeTracking

        /// <inheritdoc />
        public void AcceptChanges()
        {
            lock (_new)
            {
                _new.Clear();
                _changed.Clear();
                _deleted.Clear();
            }
        }

        /// <inheritdoc />
        public bool IsChanged => _changed.Count > 0 || _deleted.Count > 0;

        #endregion

        public void RejectChanges()
        {
            lock (_new)
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

        #region Convenience mappers

        public IReadOnlyList<T> New => _new;
        public IReadOnlyList<T> Modified => _changed;
        public IReadOnlyList<T> Deleted => _deleted;

        #endregion

        #region Extensions
        /// <summary>
        /// When submitting a dataset to the PAS the ID should be unique to be valid for progress.  Negating to mark new records.
        /// </summary>
        /// <returns></returns>
        public CDO_Table<T> NegateNewIds()
        {
            var count = -1;
            if (New != null && New.Count > 1)
            {
                foreach (T record in New)
                {
                    //Temp-table defined with "like" takes the indices from the table and thus need to be unique
                    //add a negative generated id
                    record[string.IsNullOrEmpty(record.primaryKey) ? "ID" : record.primaryKey] = count--;
                }
            }

            return this;
        }

        public CDO_Table<T> RenumberNegativeIds()
        {
            var count = -1;
            if (Count > 0)
            {
                foreach (T record in _list.Where(i => i[string.IsNullOrEmpty(i.primaryKey) ? "ID" : i.primaryKey] < 0))
                {
                    //Temp-table defined with "like" takes the indices from the table and thus need to be unique
                    //add a negative generated id
                    record[string.IsNullOrEmpty(record.primaryKey) ? "ID" : record.primaryKey] = count--;
                }
            }

            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="relTable"></param>
        /// <param name="relId"></param>
        /// <returns></returns>
        public CDO_Table<T> RelateNewTo(string relTable, string relId)
        {
            foreach (T record in New)
            {
                record["RelTable"] = relTable;
                record["RelID"] = relId;
            }

            return this;
        }
        #endregion
    }
}