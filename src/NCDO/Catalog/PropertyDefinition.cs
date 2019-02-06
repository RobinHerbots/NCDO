using System;
using System.Json;
using NCDO.Extensions;

namespace NCDO.Catalog
{
    public class PropertyDefinition : IPropertyDefinition
    {
        private JsonValue _propertyDefinition;

        public PropertyDefinition(JsonValue propertyDefinition)
        {
            _propertyDefinition = propertyDefinition;
        }


        #region Implementation of IPropertyDefinition

        /// <inheritdoc />
        public string SemanticType
        {
            get
            {
                var semanticTypeValue = _propertyDefinition.Get("type");
                return string.IsNullOrEmpty(semanticTypeValue) ? "Public" : (string) semanticTypeValue;
            }
        }

        /// <inheritdoc />
        public string Name => _propertyDefinition.Get("name");

        /// <inheritdoc />
        public Type Type
        {
            get
            {
                var typeValue = _propertyDefinition.Get("type");
                return string.IsNullOrEmpty(typeValue) ? typeof(string) : Type.GetType(typeValue);
            }
        }

        /// <inheritdoc />
        public string ABLType => _propertyDefinition.Get("ablType");

        /// <inheritdoc />
        public string Default => _propertyDefinition.Get("default")?.ToString();

        /// <inheritdoc />
        public string Title => _propertyDefinition.Get("title");

        /// <inheritdoc />
        public bool Required
        {
            get
            {
                var isRequiredValue = _propertyDefinition.Get("required");
                return isRequiredValue.JsonType != JsonType.String && (bool) isRequiredValue;
            }
        }

        /// <inheritdoc />
        public string Format => _propertyDefinition.Get("format");

        /// <summary>
        /// Optional for Field with type “array”.
        ///<ExtentValue> is an integer value specifying number of array elements.
        /// </summary>
        public int MaxItems => _propertyDefinition.Get("maxItems");

        public JsonValue Items => _propertyDefinition.Get("items");

        #endregion
    }
}