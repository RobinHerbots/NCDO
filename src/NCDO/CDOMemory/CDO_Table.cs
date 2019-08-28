using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Json;
using System.Linq;
using System.Runtime.CompilerServices;
using NCDO.Definitions;
using NCDO.Extensions;
using NCDO.Interfaces;

namespace NCDO.CDOMemory
{
    public partial class CDO_Table<T> : JsonArray, IList<T>, INotifyCollectionChanged, IChangeTracking
        where T : CDO_Record, new()
    {
        protected Dictionary<string, T> _list;
        internal Dictionary<string, T> _new = new Dictionary<string, T>();
        internal Dictionary<string, T> _changed = new Dictionary<string, T>();
        internal Dictionary<string, T> _deleted = new Dictionary<string, T>();

        public CDO_Table(params T[] items) : this((IEnumerable<T>) items)
        {
        }

        public CDO_Table(IEnumerable<T> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            _list = new Dictionary<string, T>();
            foreach (var record in items)
            {
                record.PropertyChanged += Item_PropertyChanged;
                _list.Add(record.GetId(), record);
            }
        }

        public CDO_Table(IEnumerable<JsonObject> items) : this(items as IEnumerable<JsonValue>)
        {
        }

        public CDO_Table(IEnumerable<JsonValue> items) : this(items.Select(i =>
        {
            var record = new T();
            foreach (KeyValuePair<string, JsonValue> keyValuePair in i)
            {
                record.Add(keyValuePair.Key, keyValuePair.Value, false);
            }

            return record;
        }))
        {
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
        private void _internalAdd(T item, MergeMode mergeMode, bool notify = true)
        {
            lock (_list)
            {
                if (!_list.ContainsKey(item.GetId()))
                {
                    _list.Add(item.GetId(), item);
                    item.PropertyChanged -= Item_PropertyChanged;
                    item.PropertyChanged += Item_PropertyChanged;
                    if (notify)
                        OnCollectionChanged(NotifyCollectionChangedAction.Add, new[] {item});
                }
                else
                {
                    switch (mergeMode)
                    {
                        case MergeMode.Empty:
                            break;
                        case MergeMode.Append:
                            throw new CDOException($"Duplicate record with ID {item.GetId()}");
                        case MergeMode.Merge:
                            var index = IndexOf(item);
                            if (Merge(this[index], item) && notify)
                                OnCollectionChanged(NotifyCollectionChangedAction.Move, new[] {this[index]});
                            break;
                        case MergeMode.Replace:
                            _list[item.GetId()] = item;
                            if (notify)
                                OnCollectionChanged(NotifyCollectionChangedAction.Replace, new[] {item});
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
            var oldList = _list.ToDictionary(i => i.Key, i => i.Value);
            _list.Clear();
            OnCollectionChanged(NotifyCollectionChangedAction.Reset, oldList.Values.ToArray());
        }

        public bool Contains(T item) => _list.ContainsKey(item.GetId());

        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.Values.CopyTo(array, arrayIndex);
        }

        public override int Count => _list.Count;

        public IEnumerator<T> GetEnumerator() => _list.Select(i => i.Value).GetEnumerator();

        public int IndexOf(T item)
        {
            return _list.Keys.ToList().IndexOf(item.GetId());
        }

        public void Insert(int index, T item)
        {
            if (_list.ContainsKey(item.GetId()))
            {
                _list[item.GetId()] = item;
                OnCollectionChanged(NotifyCollectionChangedAction.Move, new[] {item});
            }
            else
            {
                _list.Add(item.GetId(), item);
                OnCollectionChanged(NotifyCollectionChangedAction.Add, new[] {item});
            }
        }

        public new bool IsReadOnly => false;

        public new T this[int index]
        {
            get => _list[_list.Keys.ToList()[index]];
            set => Insert(index, value);
        }

        public bool Remove(T item)
        {
            OnCollectionChanged(NotifyCollectionChangedAction.Remove, item);
            return _list.Remove(item.GetId());
        }

        public new void RemoveAt(int index)
        {
            var item = _list[_list.Keys.ToList()[index]];
            OnCollectionChanged(NotifyCollectionChangedAction.Remove, item);
            _list.Remove(item);
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
                _internalAdd(item, mergeMode, notify);
            }
        }

        #region Overrides of JsonValue

        /// <inheritdoc />
        #endregion

        public override void Save(TextWriter textWriter)
        {
            lock (_list)
            {
                NegateNewIds().RenumberNegativeIds();
                base.Save(textWriter);
            }
        }

        #region Implementation of INotifyCollectionChanged

        /// <inheritdoc />
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        protected virtual void OnCollectionChanged(NotifyCollectionChangedAction action, params T[] items)
        {
            switch (action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddNew(_new, items, DataRowState.Created);
                    break;
                case NotifyCollectionChangedAction.Move:
                    var modifyList = items?.Where(i => !_new.ContainsKey(i.GetId()));
                    AddNew(_changed, modifyList, DataRowState.Modified);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    var removeList = items.Where(i => !_new.ContainsKey(i.GetId()));
                    //cleanup other dicts
                    foreach (var item in items)
                    {
                        var id = item.GetId();
                        _changed.Remove(id);
                        _new.Remove(id);
                    }

                    AddNew(_deleted, removeList, DataRowState.Deleted);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    var replaceList = items.Where(i => !_new.ContainsKey(i.GetId()));
                    AddNew(_changed, replaceList, DataRowState.Modified);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    _changed.Clear();
                    _new.Clear();
                    _deleted.Clear();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }


            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, items));
        }

        private void OnCollectionChanged(object sender, PropertyChangedEventArgs e)
        {
            OnCollectionChanged(NotifyCollectionChangedAction.Move, new[] {(T) sender});
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

        private void AddNew(IDictionary<string, T> list, IEnumerable<T> items, DataRowState rowState)
        {
            if (items != null)
            {
                foreach (var item in items)
                {
                    lock (list)
                    {
                        if (!list.ContainsKey(item.GetId()))
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
                            list.Add(itemClone.GetId(), itemClone);
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
                foreach (var recordKey in _new.Keys)
                {
                    _list.Remove(recordKey);
                }

                foreach (var record in _changed.Values)
                {
                    record.RejectRowChanges();
                }

                _changed.Clear();
                AddRange(_deleted.Values);
                _deleted.Clear();
            }
        }

        #region Convenience mappers

        public IReadOnlyDictionary<string, T> New => _new;
        public IReadOnlyDictionary<string, T> Modified => _changed;
        public IReadOnlyDictionary<string, T> Deleted => _deleted;

        #endregion

        #region Extensions

        /// <summary>
        /// When submitting a dataset to the PAS the ID should be unique to be valid for progress.  Negating to mark new records.
        /// </summary>
        /// <returns></returns>
        internal CDO_Table<T> NegateNewIds()
        {
            var count = -1;
            if (New != null)
            {
                if (New.Count == 1)
                {
                    New.First().Value.SetId("0");
                }
                else if (New.Count > 1)
                {
                    foreach (T record in New.Values)
                    {
                        //Temp-table defined with "like" takes the indices from the table and thus need to be unique
                        //add a negative generated id
                        record.SetId((count--).ToString());
                    }
                }
            }

            return this;
        }

        internal CDO_Table<T> RenumberNegativeIds()
        {
            var count = -1;
            if (Count > 0)
            {
                foreach (var record in _list.Where(i => i.Key.StartsWith("-")))
                {
                    //Temp-table defined with "like" takes the indices from the table and thus need to be unique
                    //add a negative generated id
                    record.Value.SetId((count--).ToString());
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
            foreach (T record in New.Values)
            {
                record["RelTable"] = relTable;
                record["RelID"] = relId;
            }

            return this;
        }

        #endregion
    }
}