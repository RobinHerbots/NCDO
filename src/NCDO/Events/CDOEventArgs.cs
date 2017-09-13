using System;
using System.Collections.Generic;
using System.Text;
using NCDO.Interfaces;

namespace NCDO
{
    public class CDOEventArgs : EventArgs
    {
        public ICDOSession Session { get; set; }
        public ICDORequest Request { get; set; }

    }
}
