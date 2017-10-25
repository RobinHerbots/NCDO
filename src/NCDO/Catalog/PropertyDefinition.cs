using System;
using System.Collections.Generic;
using System.Json;
using System.Text;
using NCDO.Extensions;

namespace NCDO.Catalog
{
    public class PropertyDefinition : IPropertyDefinition
    {
        public PropertyDefinition(JsonValue propertyDefinition)
        {
            var semanticTypeValue = propertyDefinition.Get("type");
            SemanticType = string.IsNullOrEmpty(semanticTypeValue) ? "Public" : (string)semanticTypeValue;
            Name = propertyDefinition.Get("name");
            var typeValue = propertyDefinition.Get("type");
            Type = string.IsNullOrEmpty(typeValue) ? typeof(string) : Type.GetType(typeValue);
            ABLType = propertyDefinition.Get("ablType");
            Default = propertyDefinition.Get("default").ToString();
            Title = propertyDefinition.Get("title");
            var isRequiredValue = propertyDefinition.Get("required");
            Required = isRequiredValue.JsonType != JsonType.String && (bool) isRequiredValue;
            Format = propertyDefinition.Get("format");
        }


        #region Implementation of IPropertyDefinition

        /// <inheritdoc />
        public string SemanticType { get; }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public Type Type { get; }

        /// <inheritdoc />
        public string ABLType { get; }

        /// <inheritdoc />
        public string Default { get; }

        /// <inheritdoc />
        public string Title { get; }

        /// <inheritdoc />
        public bool Required { get; }

        /// <inheritdoc />
        public string Format { get; }

        #endregion
    }
}
