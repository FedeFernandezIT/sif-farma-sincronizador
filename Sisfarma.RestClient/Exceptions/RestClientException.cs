using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisfarma.RestClient.Exceptions
{
    public class RestClientException : Exception
    {
        public int HttpStatus { get; private set; }

        public string HttpStatusDescription { get; private set; }

        public RestClientException() : this(500, "Internal Server Error")
        {
        }

        public RestClientException(int httpStatus, string httpStatusDescription)
        {
            HttpStatus = httpStatus;
            HttpStatusDescription = httpStatusDescription ?? throw new ArgumentNullException(nameof(httpStatusDescription));
        }
    }
}