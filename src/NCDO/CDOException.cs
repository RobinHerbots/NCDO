using System;
using System.Runtime.Serialization;

namespace NCDO
{
    public class CDOException : Exception
    {
        public CDOException()
        {
        }

        public CDOException(string message) : base(message)
        {
        }

        public CDOException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected CDOException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
