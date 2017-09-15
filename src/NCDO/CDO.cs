using NCDO.Interfaces;
using System;
using System.Collections.Generic;
using System.Json;
using System.Text;

namespace NCDO
{
    /// <summary>
    /// Base CDO which returns data as Json objects
    /// </summary>
    public class CDO : ACloudDataObject<JsonObject>
    {
        public CDO(string respource, ICDOSession cDOSession = null, bool autoFill = false) : base(respource, cDOSession, autoFill)
        {
        }
    }
}
