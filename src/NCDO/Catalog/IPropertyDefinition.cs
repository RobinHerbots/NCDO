using System;

namespace NCDO.Catalog
{
    /// <summary>
    /// IPropertyDefinition
    /// </summary>
    public interface IPropertyDefinition
    {
        string SemanticType { get; }
        string Name { get; }
        Type Type { get; }
        string ABLType { get; }
        string Default { get; }
        string Title { get; }
        bool Required { get; }
        string Format { get; }
    }
}
