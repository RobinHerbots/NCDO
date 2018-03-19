using System;
using System.Collections.Generic;
using System.Json;
using System.Linq;
using System.Text;

namespace NCDO.Extensions
{
    public static class JsonValueExtensions
    {
        public static JsonValue Get(this JsonValue jsonValue, string key)
        {
            return jsonValue.ContainsKey(key) ? jsonValue[key] : new JsonPrimitive((string)null);
        }

        internal static void Set(this JsonValue jsonValue, string key, JsonValue value)
        {
            jsonValue[key] = value;
        }

        public static void ThrowOnErrorResponse(this JsonValue jsonValue)
        {
            if (jsonValue.ContainsKey("error"))
            {
                throw new CDOException(jsonValue.Get("error"), jsonValue.Get("error_description")) { Scope = jsonValue.Get("scope") };
            }

            if (jsonValue.ContainsKey("_errors"))
            {
                JsonArray errors = (JsonArray) jsonValue.Get("_errors");
                throw new CDOException(jsonValue.Get("_retval"),errors.FirstOrDefault().Get("_errorMsg") );
            }
        }
    }
}