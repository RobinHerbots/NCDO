using System;
using System.Json;
using NCDO.Extensions;

namespace NCDO.Catalog
{
    public class Param
    {
        private readonly JsonValue _param;

        public Param(JsonValue param)
        {
            _param = param;
        }

        /// <summary>
        /// Parameter name
        /// </summary>
        public string Name => _param.Get("name");

        public ParamType Type
        {
            get
            {
                return _param.ContainsKey("paramType")
                    ? (ParamType) Enum.Parse(typeof(ParamType), _param.Get("paramType"), true)
                    : ParamType.Ambiguous;
            }
        }

        /// <summary>
        /// Specifies parameter data type. Can be one of the following: “TABLE” “DATASET” “ARRAY” (for array of objects) json datatype: either “string”, “integer”, “number”, “boolean” ABL datatype: see OE ABL datatype mapping chart If “TABLE”, “DATASET”, or “ARRAY” is specified, then its schema needs to be specified in the resource’s dataDefinitions property.
        /// </summary>
        public string XType => _param.Get("xType");

        /// <summary>
        /// True or False. Indicates whether parameter is an array of simple datatypes. For OE, this corresponds to the EXTENT option in the ABL
        /// </summary>
        public bool IsArray => _param.Get("isArray");
    }
}