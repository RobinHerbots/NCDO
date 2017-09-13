using System;
using System.Collections.Generic;
using System.Text;
using NCDO.Interfaces;

namespace NCDO
{
    public class CDOOfflineEventArgs : CDOEventArgs
    {
        public string OfflineReason { get; set; }
    }
}
