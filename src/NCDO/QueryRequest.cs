using System;
using System.Json;
using System.Linq.Expressions;
using NCDO.Extensions;

namespace NCDO
{
    public class QueryRequest : JsonObject
    {
        public QueryRequest()
        {
            Top = Int32.MaxValue;
            Skip = -1;
        }

        /// <summary>
        ///     Filterstring used to select the records to be returned. These property settings are in the format as the property
        ///     settings in the Kendo UI DataSouce filter property object. For more information, see the filter configuration
        ///     property description in the Telerik Kendo UI DataSource documentation.
        /// </summary>
        public string ABLFilter
        {
            get => this.Get("filter");
            set => this["filter"] = value;
        }

        /// <summary>
        ///     Unique ID that the resource understands to identify a specific record.
        /// </summary>
        public string ID
        {
            get => this.Get("id");
            set => this["id"] = value;
        }

        /// <summary>
        ///     A number that specifies how many records to skip before returning (up to) a page of data. You must specify this
        ///     property together with the top property.
        /// </summary>
        public int Skip
        {
            get => this.Get("skip");
            set => this["skip"] = value;
        }

        /// <summary>
        ///     An expression that specifies how to sort the records to be returned.
        /// </summary>
        public string Sort
        {
            get => this.Get("sort");
            set => this["sort"] = value;
        }

        /// <summary>
        ///     A string specifying the name of a table reference in the CDO. This property is required when the CDO represents a
        ///     multi-table resource and the filter property Object is also specified with filter information.
        /// </summary>
        public string TableRef
        {
            get => this.Get("tableRef");
            set => this["tableRef"] = value;
        }

        /// <summary>
        ///     A number that specifies how many records (the page size) to return in a single page of data after using skip. You
        ///     must specify this property together with the skip property. The final page of a larger result set can contain a
        ///     smaller number of records than top specifies.
        /// </summary>
        public int Top
        {
            get => this.Get("top");
            set => this["top"] = value;
        }

        #region Overrides of JsonValue

        /// <inheritdoc />
        public override string ToString()
        {
            return ToString(null);
        }

        public string ToString(string capabilities)
        {
            if (string.IsNullOrEmpty(capabilities))
                return string.IsNullOrEmpty(ABLFilter) ? "" : ABLFilter.Trim('"');

            if (!string.IsNullOrEmpty(ID) &&
                capabilities.IndexOf("id", StringComparison.InvariantCultureIgnoreCase) != -1)
                return $"ID={ID}";

            var filter = new JsonObject();
            if (!string.IsNullOrEmpty(ABLFilter) &&
                capabilities.IndexOf("ablFilter", StringComparison.InvariantCultureIgnoreCase) != -1)
                filter.Add("ablFilter", ABLFilter);
            if (!string.IsNullOrEmpty(null) &&
                capabilities.IndexOf("sqlQuery", StringComparison.InvariantCultureIgnoreCase) != -1)
                filter.Add("sqlQuery", "");
            if (!string.IsNullOrEmpty(Sort) &&
                capabilities.IndexOf("orderBy", StringComparison.InvariantCultureIgnoreCase) != -1)
                filter.Add("orderBy", Sort);
            if (default(int) != Skip && capabilities.IndexOf("skip", StringComparison.InvariantCultureIgnoreCase) != -1)
                filter.Add("skip", Skip);
            if (default(int) != Top && capabilities.IndexOf("top", StringComparison.InvariantCultureIgnoreCase) != -1)
                filter.Add("top", Top);

            return filter.ToString();
        }

        #endregion
    }
}