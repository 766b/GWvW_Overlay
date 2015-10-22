using System;
using System.Net;

namespace ArenaNET
{
    public class ANetException : Exception
    {
        public HttpStatusCode StatusCode { get; private set; }
        public ANetException(HttpStatusCode status)
            : base()
        {
            StatusCode = status;
        }

        public ANetException(HttpStatusCode status, String message)
            : base(message)
        {
            StatusCode = status;
        }

        public ANetException(HttpStatusCode status, String message, Exception innerException)
            : base(message, innerException)
        {
            StatusCode = status;
        }


    }
}