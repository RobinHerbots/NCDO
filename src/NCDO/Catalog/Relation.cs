using System.Collections.Generic;
using System.Json;
using NCDO.Extensions;

namespace NCDO.Catalog
{
    public class Relation
    {
        public Relation(JsonValue relation)
        {
            RelationName = relation.Get("relationName");
            ParentName = relation.Get("parentName");
            ChildName = relation.Get("childName");
            if (relation.ContainsKey("relationFields"))
            {
                RelationFields = new List<KeyValuePair<string, string>>();
                foreach (JsonObject relationField in relation.Get("relationFields"))
                {
                    foreach (string key in relationField.Keys)
                    {
                        RelationFields.Add(new KeyValuePair<string, string>(key, relationField[key]));
                    }
                }
            }
        }

        /// <summary>
        /// Relation Name
        /// </summary>
        public string RelationName { get; set; }
        /// <summary>
        /// Name of parent table in relationship
        /// </summary>
        public string ParentName { get; set; }
        /// <summary>
        /// Name of child table in relationship
        /// </summary>
        public string ChildName { get; set; }
        /// <summary>
        /// Array of parent field name and child field name pairs. Can be used by the CDO to retrieve data for the child table based upon an equality match between all pairs of fields listed in this array.
        /// key = parentFieldName, value : childFieldName
        /// </summary>
        public List<KeyValuePair<string, string>> RelationFields { get; set; }
    }
}
