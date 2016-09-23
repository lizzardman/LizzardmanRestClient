using System;
using System.Net;

namespace LizzardmanRestClient
{
    public class RestClientException : Exception
    {
        public HttpStatusCode StatusCode { get; set; }
        public bool IsNetwork { get; set; }
        new public Exception InnerException { get; set; }
        new public string Message { get; set; }
    }
}
