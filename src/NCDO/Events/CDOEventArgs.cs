using System;
using System.Collections.Generic;
using System.Text;
using NCDO.CDOMemory;
using NCDO.Interfaces;

namespace NCDO
{
    public class CDOEventArgs : EventArgs
    {
        public ICDOSession Session { get; set; }
        public ICDORequest Request { get; set; }
    }
    public class CDOEventArgs<T, D> : CDOEventArgs 
        where T : class
        where D: CDO_Dataset, new()
    {
        public ICloudDataRecord Record { get; set; }
        public ICloudDataObject<T, D> CDO { get; internal set; }
    }
}
