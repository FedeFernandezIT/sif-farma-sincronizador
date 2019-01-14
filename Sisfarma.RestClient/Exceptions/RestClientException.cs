using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Sisfarma.RestClient.Exceptions
{
    public class RestClientException : Exception
    {
        public HttpStatusCode HttpStatus { get; private set; }

        public string HttpStatusDescription { get; private set; }

        public RestClientException() : this(HttpStatusCode.InternalServerError, "Internal Server Error")
        {
        }

        public RestClientException(HttpStatusCode httpStatus, string httpStatusDescription)
        {
            HttpStatus = httpStatus;
            HttpStatusDescription = httpStatusDescription ?? throw new ArgumentNullException(nameof(httpStatusDescription));
        }        

        public static RestClientException Create(HttpStatusCode statusCode, string statusDescription)
        {            
            switch (statusCode)
            {
                case HttpStatusCode.NotFound:
                    return new RestClientNotFoundException(statusDescription);
                
                default:
                    return new RestClientException(statusCode, statusDescription);
            }
        }
    }
}