// Licensed to the .NET Foundation under one or more agreements.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Json;
using System.Linq;

namespace NCDO.CDOMemory
{
    public class CDO_Table : JsonValue, IList<CDO_Record>
    {
        private readonly List<CDO_Record> _list;

        public CDO_Table(params CDO_Record[] items)
        {
            _list = new List<CDO_Record>();
            AddRange(items);
        }

        public CDO_Table(IEnumerable<CDO_Record> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            _list = new List<CDO_Record>(items);
        }

        public CDO_Table(IEnumerable<JsonObject> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            _list = new List<CDO_Record>(items.Select(i => new CDO_Record(i)));
        }

        public override int Count => _list.Count;

        public bool IsReadOnly => false;

        public new CDO_Record this[int index]
        {
            get => _list[index];
            set => _list[index] = value;
        }

        public override JsonType JsonType => JsonType.Array;

        public void Add(CDO_Record item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            _list.Add(item);
        }

        public void AddRange(IEnumerable<CDO_Record> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            _list.AddRange(items);
        }

        public void AddRange(params CDO_Record[] items)
        {
            if (items != null)
            {
                _list.AddRange(items);
            }
        }

        public void Clear() => _list.Clear();

        public bool Contains(CDO_Record item) => _list.Contains(item);

        public void CopyTo(CDO_Record[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);

        public int IndexOf(CDO_Record item) => _list.IndexOf(item);

        public void Insert(int index, CDO_Record item) => _list.Insert(index, item);

        public bool Remove(CDO_Record item) => _list.Remove(item);

        public void RemoveAt(int index) => _list.RemoveAt(index);

        public override void Save(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            stream.WriteByte((byte)'[');

            for (int i = 0; i < _list.Count; i++)
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

        IEnumerator<CDO_Record> IEnumerable<CDO_Record>.GetEnumerator() => _list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();
    }
}