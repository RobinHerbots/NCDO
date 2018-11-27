using System.Json;

namespace NCDO.Extensions
{
    public static class JsonValueExtensions
    {
        public static string AsString(this JsonValue jsonValue)
        {
            return jsonValue is JsonPrimitive jsonPrimitiveValue && jsonPrimitiveValue.JsonType == JsonType.String
                ? (string) jsonValue
                : jsonValue.ToString();
        }
        
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
            if (jsonValue.ContainsKey("error")) //pas error
            {
                throw new CDOException(jsonValue.Get("error"), jsonValue.Get("error_description")) { Scope = jsonValue.Get("scope") };
            }

            if (jsonValue.ContainsKey("_errors")) //progress error
            {
                string code = "";
                string message = jsonValue.Get("_retVal");

                var errors = (JsonArray)jsonValue.Get("_errors");
                foreach (var error in errors)
                {
                    code = error.Get("_errorNum").ToString();
                    if (string.IsNullOrEmpty(message)) message = error.Get("_errorMsg");
                    break;
                }
                throw new CDOException(code, message);
            }
        }
    }
}