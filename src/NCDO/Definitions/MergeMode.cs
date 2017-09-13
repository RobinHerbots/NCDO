namespace NCDO.Definitions
{
    public enum MergeMode 
    {
        /// <summary>
        /// Empties the CDO memory and loads data.
        /// </summary>
        Empty,
        /// <summary>
        /// Adds the data returned by the invoke operation to the existing data in the CDO memory. The method would throw an error if a duplicate key is found.
        /// </summary>
        Append,
        /// <summary>
        /// Merges the data returned by the invoke operation. If a duplicate key is found, the record will not be added to the CDO memory.
        /// </summary>
        Merge,
        /// <summary>
        /// Merges the data returned by the invoke operation. Records with a duplicate key are replaced.
        /// </summary>
        Replace
    }
}
