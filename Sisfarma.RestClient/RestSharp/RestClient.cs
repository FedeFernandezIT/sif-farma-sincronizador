using RestSharp.Authenticators;
using System;
using System.Threading.Tasks;
using RSharp = RestSharp;

namespace Sisfarma.RestClient.RestSharp
{
    public class RestClient : BaseRestClient, IRestClient
    {
        private RSharp.RestClient _restClient;
        private RSharp.RestRequest _request;

        public RestClient()
        {
            _restClient = new RSharp.RestClient();
            _request = new RSharp.RestRequest();
        }

        public RestClient(Uri baseAddress) : base(baseAddress)
        {
            _restClient = new RSharp.RestClient(baseAddress);
            _request = new RSharp.RestRequest();
        }

        public RestClient(Uri baseAddress, Uri resource) : base(baseAddress, resource)
        {
            _restClient = new RSharp.RestClient(baseAddress);
            _request = new RSharp.RestRequest();
        }

        public IRestClient BaseAddress(string url)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url));

            _baseAddress = new Uri(url);
            _restClient.BaseUrl = _baseAddress;
            return this;
        }

        public IRestClient Resource(string resource)
        {
            if (string.IsNullOrEmpty(resource))
                throw new ArgumentNullException(nameof(resource));

            _resource = new Uri(resource, UriKind.Relative);

            if (_resource.IsAbsoluteUri)
                throw new ArgumentException("No es una ruta relativa", nameof(resource));

            _request.Resource = resource;
            return this;
        }

        public IRestClient UseAuthenticationBasic(string username, string password)
        {
            _restClient.Authenticator = new HttpBasicAuthenticator(username, password);
            return this;
        }

        public async Task<T> SendGetAsync<T>()
        {
            var response = await _restClient.ExecuteGetTaskAsync(_request);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(response.Content);
        }

        public T SendGet<T>()
        {
            Console.WriteLine($"GET: {_resource} ...");
            var response = _restClient.Execute(_request, RSharp.Method.GET);
            Console.WriteLine($"GET: {_resource} finalizado");
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(response.Content);
        }

        public async Task<T> SendPostAsync<T>(object body)
        {
            var response = await _restClient.ExecutePostTaskAsync(_request.AddJsonBody(body));
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(response.Content);
        }

        public T SendPost<T>(object body)
        {
            Console.WriteLine($"POST: {_resource} ...");
            var response = _restClient.Execute(_request.AddJsonBody(body), RSharp.Method.POST);
            Console.WriteLine($"POST: {_resource} finalizado");
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(response.Content);
        }

        public void SendPost(object body)
        {
            Console.WriteLine($"POST: {_resource} ...");
            _restClient.Execute(_request.AddJsonBody(body), RSharp.Method.POST);
            Console.WriteLine($"POST: {_resource} finalizado");
        }

        public void SendPut()
        {
            Console.WriteLine($"PUT: {_resource} ...");
            _restClient.Execute(_request, RSharp.Method.PUT);
            Console.WriteLine($"PUT: {_resource} finalizado");
        }

        public void SendPut(object body)
        {
            Console.WriteLine($"PUT: {_resource} ...");
            _restClient.Execute(_request.AddJsonBody(body), RSharp.Method.PUT);
            Console.WriteLine($"PUT: {_resource} finalizado");
        }

        public T SendPut<T>(object body)
        {
            Console.WriteLine($"PUT: {_resource} ...");
            var response = _restClient.Execute(_request.AddJsonBody(body), RSharp.Method.PUT);
            Console.WriteLine($"PUT: {_resource} finalizado");
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(response.Content);
        }

        public void SendDelete(object body)
        {
            Console.WriteLine($"DELETE: {_resource} ...");
            _restClient.Execute(_request.AddJsonBody(body), RSharp.Method.DELETE);
            Console.WriteLine($"DELETE: {_resource} finalizado");
        }
    }
}