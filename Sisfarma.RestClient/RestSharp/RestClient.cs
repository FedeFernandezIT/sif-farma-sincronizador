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
            var response = _restClient.Execute(_request, RSharp.Method.GET);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(response.Content);
        }

        public async Task<T> SendPostAsync<T>(object body)
        {
            var response = await _restClient.ExecutePostTaskAsync(_request.AddJsonBody(body));
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(response.Content);
        }

        public T SendPost<T>(object body)
        {
            var response = _restClient.Execute(_request.AddJsonBody(body), RSharp.Method.POST);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(response.Content);
        }

        public void SendPost(object body)
        {
            _restClient.Execute(_request.AddJsonBody(body), RSharp.Method.POST);
        }

        public void SendPut()
        {
            _restClient.Execute(_request, RSharp.Method.PUT);
        }

        public void SendPut(object body)
        {
            _restClient.Execute(_request.AddJsonBody(body), RSharp.Method.PUT);
        }

        public T SendPut<T>(object body)
        {
            var response = _restClient.Execute(_request.AddJsonBody(body), RSharp.Method.PUT);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(response.Content);
        }
    }
}