using System;
using System.Collections.Generic;
using System.Json;
using System.Text;

namespace NCDO.Extensions
{
    public static class JsonValueExtensions
    {
        public static JsonValue Get(this JsonValue jsonValue, string key)
        {
            return jsonValue.ContainsKey(key) ? jsonValue[key] : new JsonPrimitive((string)null);
        }
    }
}
