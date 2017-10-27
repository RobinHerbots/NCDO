using System;
using NCDO.CDOMemory;
using NCDO.Interfaces;

namespace NCDO.Events
{
    public class CDOEventArgs : EventArgs
    {
        public ICDOSession Session { get; set; }
        public ICDORequest Request { get; set; }
    }
    public class CDOEventArgs<T, D, R> : CDOEventArgs
        where T : class
        where D : CDO_Dataset, new()
        where R : ICloudDataRecord
    {
        public ICloudDataRecord Record { get; set; }
        public ICloudDataObject<T, D, R> CDO { get; internal set; }
    }
}
