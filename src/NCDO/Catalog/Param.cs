using System;
using System.Json;
using NCDO.Extensions;

namespace NCDO.Catalog
{
    public class Param
    {
        public Param(JsonValue param)
        {
            Name = param.Get("name");
            if (param.ContainsKey("paramType"))
                Type = (ParamType)Enum.Parse(typeof(ParamType), param.Get("paramType"), true);
            XType = param.Get("xType");
            IsArray = param.Get("isArray");
        }
        /// <summary>
        /// Parameter name
        /// </summary>
        public string Name { get; }
        public ParamType Type { get; }
        /// <summary>
        /// Specifies parameter data type. Can be one of the following: “TABLE” “DATASET” “ARRAY” (for array of objects) json datatype: either “string”, “integer”, “number”, “boolean” ABL datatype: see OE ABL datatype mapping chart If “TABLE”, “DATASET”, or “ARRAY” is specified, then its schema needs to be specified in the resource’s dataDefinitions property.
        /// </summary>
        public string XType { get; }
        /// <summary>
        /// True or False. Indicates whether parameter is an array of simple datatypes. For OE, this corresponds to the EXTENT option in the ABL
        /// </summary>
        public bool IsArray { get; }
    }
}
