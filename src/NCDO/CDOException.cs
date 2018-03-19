using System;
using System.Runtime.Serialization;

namespace NCDO
{
    public class CDOException : Exception
    {
        public string Code { get; set; }
        public string Scope { get; set; }


        public CDOException()
        {
        }

        public CDOException(string message) : base(message)
        {
        }

        public CDOException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public CDOException(string code, string message, Exception innerException= null) : base(message, innerException)
        {
            Code = code;
        }

        protected CDOException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
