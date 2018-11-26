using System.Collections.Generic;
using NCDO.Catalog;

namespace NCDO.Interfaces
{
    public interface ICDOCatalog
    {
        /// <summary>
        /// Catalog version
        /// </summary>
        string Version { get; }
        /// <summary>
        /// Date when the catalog is generated
        /// </summary>
        string LastModified { get; }

        IList<Service> Services { get; }
    }
}
