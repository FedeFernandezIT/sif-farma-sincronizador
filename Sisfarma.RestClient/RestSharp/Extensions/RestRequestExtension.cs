using RestSharp;
using System;
using System.Linq;

namespace Sisfarma.RestClient.RestSharp.Extensions
{
    public static class RestRequestExtension
    {
        public static Request ToRestClientRequest(this IRestRequest @this, Uri baseAddress)
        {
            var body = @this.Parameters
                .FirstOrDefault(p => p.Type == ParameterType.RequestBody)?
                .ToString() ?? null;

            var url = $"{baseAddress}/{@this.Resource}";

            return new Request(url, @this.Method.ToString(), body);
        }
    }
}
