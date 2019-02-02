using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisfarma.RestClient
{
    public class Response
    {
        public string Content { get; private set; }

        public int StatusHttp { get; set; }

        public Response(string content, int statusHttp)
        {
            Content = content;
            StatusHttp = statusHttp;
        }

        public override string ToString() 
            => $"{StatusHttp}{Environment.NewLine}{Content}";
    }
}
