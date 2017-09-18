using System;
using System.Collections.Generic;
using System.Json;
using System.Text;

namespace NCDO.CDOMemory
{
    public class CDO_Record : JsonObject
    {
        public CDO_Record(JsonValue jsonValue)
        {
            
        }

        public bool HasChanges { get; private set;}
    }
}
