using System.Collections.Generic;
using System.Json;
using NCDO.Extensions;

namespace NCDO.Catalog
{
    public class Relation
    {
        private JsonValue _relation;
        private List<KeyValuePair<string, string>> _relationFields;

        public Relation(JsonValue relation)
        {
            _relation = relation;
        }

        /// <summary>
        /// Relation Name
        /// </summary>
        public string RelationName => _relation.Get("relationName");

        /// <summary>
        /// Name of parent table in relationship
        /// </summary>
        public string ParentName => _relation.Get("parentName");

        /// <summary>
        /// Name of child table in relationship
        /// </summary>
        public string ChildName => _relation.Get("childName");

        /// <summary>
        /// Array of parent field name and child field name pairs. Can be used by the CDO to retrieve data for the child table based upon an equality match between all pairs of fields listed in this array.
        /// key = parentFieldName, value : childFieldName
        /// </summary>
        public List<KeyValuePair<string, string>> RelationFields
        {
            get
            {
                if (_relationFields == null)
                {
                    _relationFields = new List<KeyValuePair<string, string>>();
                    if (_relation.ContainsKey("relationFields"))
                    {
                        foreach (JsonObject relationField in _relation.Get("relationFields"))
                        {
                            foreach (string key in relationField.Keys)
                            {
                                _relationFields.Add(new KeyValuePair<string, string>(key, relationField[key]));
                            }
                        }
                    }
                }

                return _relationFields;
            }
        }
    }
}