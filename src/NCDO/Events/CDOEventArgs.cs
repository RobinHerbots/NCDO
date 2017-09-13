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
    public class CDOEventArgs<T> : CDOEventArgs where T : class
    {
        public ICloudDataRecord<T> Record { get; set; }
        public ICloudDataObject<T> CDO { get; internal set; }
    }
}
