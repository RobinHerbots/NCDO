// Licensed to the .NET Foundation under one or more agreements.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Json;
using System.Text;
using NCDO.Extensions;
using JsonPair = System.Collections.Generic.KeyValuePair<string, System.Json.JsonValue>;
using JsonPairEnumerable = System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, System.Json.JsonValue>>;

namespace NCDO.CDOMemory
{
    public class CDO_Record : JsonObject, IDictionary<string, JsonValue>, ICollection<JsonPair>
    {
        // Use SortedDictionary to make result of ToString() deterministic
        private readonly SortedDictionary<string, JsonValue> _map = new SortedDictionary<string, JsonValue>(StringComparer.Ordinal);

        #region Constructor
        public CDO_Record(params JsonPair[] items)
        {
            if (items != null)
            {
                AddRange(items);
            }
        }

        public CDO_Record(JsonPairEnumerable items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            AddRange(items);
        }

        public CDO_Record() : base()
        {

        }
        #endregion

        #region Internal Properties
        /// <summary>
        /// An internal field for the CDO that is provided to find a given record in its memory.
        /// </summary>
        internal string _id => this.Get("_id");
        /// <summary>
        /// Used by the CDO to do automatic data mapping for any error string passed back from backend with before-imaging data
        /// </summary>
        internal string _errorString => this.Get("_errorString");
        #endregion

        public override int Count => _map.Count;

        public IEnumerator<JsonPair> GetEnumerator() => _map.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _map.GetEnumerator();

        public new JsonValue this[string key]
        {
            get { return _map[key]; }
            set { _map[key] = value; }
        }

        public override JsonType JsonType => JsonType.Object;

        public ICollection<string> Keys => _map.Keys;

        public ICollection<JsonValue> Values => _map.Values;

        public void Add(string key, JsonValue value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            _map.Add(key, value);
        }

        public void Add(JsonPair pair) => Add(pair.Key, pair.Value);

        public void AddRange(JsonPairEnumerable items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            foreach (var pair in items)
            {
                _map.Add(pair.Key, pair.Value);
            }
        }

        public void AddRange(params JsonPair[] items) => AddRange((JsonPairEnumerable)items);

        public void Clear() => _map.Clear();

        bool ICollection<JsonPair>.Contains(JsonPair item) => (_map as ICollection<JsonPair>).Contains(item);

        bool ICollection<JsonPair>.Remove(JsonPair item) => (_map as ICollection<JsonPair>).Remove(item);

        public override bool ContainsKey(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return _map.ContainsKey(key);
        }

        public void CopyTo(JsonPair[] array, int arrayIndex) => (_map as ICollection<JsonPair>).CopyTo(array, arrayIndex);

        public bool Remove(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return _map.Remove(key);
        }

        bool ICollection<JsonPair>.IsReadOnly => false;

        public override void Save(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            stream.WriteByte((byte)'{');

            foreach (JsonPair pair in _map)
            {
                stream.WriteByte((byte)'"');
                byte[] bytes = Encoding.UTF8.GetBytes(EscapeString(pair.Key));
                stream.Write(bytes, 0, bytes.Length);
                stream.WriteByte((byte)'"');
                stream.WriteByte((byte)',');
                stream.WriteByte((byte)' ');
                if (pair.Value == null)
                {
                    stream.WriteByte((byte)'n');
                    stream.WriteByte((byte)'u');
                    stream.WriteByte((byte)'l');
                    stream.WriteByte((byte)'l');
                }
                else
                {
                    pair.Value.Save(stream);
                }
            }

            stream.WriteByte((byte)'}');
        }

        public bool TryGetValue(string key, out JsonValue value) => _map.TryGetValue(key, out value);

        #region Private
        // Characters which have to be escaped:
        // - Required by JSON Spec: Control characters, '"' and '\\'
        // - Broken surrogates to make sure the JSON string is valid Unicode
        //   (and can be encoded as UTF8)
        // - JSON does not require U+2028 and U+2029 to be escaped, but
        //   JavaScript does require this:
        //   http://stackoverflow.com/questions/2965293/javascript-parse-error-on-u2028-unicode-character/9168133#9168133
        // - '/' also does not have to be escaped, but escaping it when
        //   preceeded by a '<' avoids problems with JSON in HTML <script> tags
        private static bool NeedEscape(string src, int i)
        {
            char c = src[i];
            return c < 32 || c == '"' || c == '\\'
                // Broken lead surrogate
                || (c >= '\uD800' && c <= '\uDBFF' &&
                    (i == src.Length - 1 || src[i + 1] < '\uDC00' || src[i + 1] > '\uDFFF'))
                // Broken tail surrogate
                || (c >= '\uDC00' && c <= '\uDFFF' &&
                    (i == 0 || src[i - 1] < '\uD800' || src[i - 1] > '\uDBFF'))
                // To produce valid JavaScript
                || c == '\u2028' || c == '\u2029'
                // Escape "</" for <script> tags
                || (c == '/' && i > 0 && src[i - 1] == '<');
        }
        internal static string EscapeString(string src)
        {
            if (src != null)
            {
                for (int i = 0; i < src.Length; i++)
                {
                    if (NeedEscape(src, i))
                    {
                        var sb = new StringBuilder();
                        if (i > 0)
                        {
                            sb.Append(src, 0, i);
                        }
                        return DoEscapeString(sb, src, i);
                    }
                }
            }

            return src;
        }

        private static string DoEscapeString(StringBuilder sb, string src, int cur)
        {
            int start = cur;
            for (int i = cur; i < src.Length; i++)
                if (NeedEscape(src, i))
                {
                    sb.Append(src, start, i - start);
                    switch (src[i])
                    {
                        case '\b': sb.Append("\\b"); break;
                        case '\f': sb.Append("\\f"); break;
                        case '\n': sb.Append("\\n"); break;
                        case '\r': sb.Append("\\r"); break;
                        case '\t': sb.Append("\\t"); break;
                        case '\"': sb.Append("\\\""); break;
                        case '\\': sb.Append("\\\\"); break;
                        case '/': sb.Append("\\/"); break;
                        default:
                            sb.Append("\\u");
                            sb.Append(((int)src[i]).ToString("x04"));
                            break;
                    }
                    start = i + 1;
                }
            sb.Append(src, start, src.Length - start);
            return sb.ToString();
        }
        #endregion
    }
}