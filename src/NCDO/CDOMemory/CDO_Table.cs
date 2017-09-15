using System;
using System.Collections.Generic;
using System.Json;
using System.Text;

namespace NCDO.CDOMemory
{
    public class CDO_Table : JsonArray
    {
        public CDO_Table(IEnumerable<JsonValue> jsonValue)
        {
            AddRange(jsonValue);
        }

        public bool HasChanges { get; private set;}
    }
}
