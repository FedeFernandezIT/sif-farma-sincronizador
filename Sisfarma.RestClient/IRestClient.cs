using System.Threading.Tasks;

namespace Sisfarma.RestClient
{
    public interface IRestClient
    {
        IRestClient BaseAddress(string ulr);

        IRestClient Resource(string resource);

        IRestClient UseAuthenticationBasic(string username, string password);

        Task<T> SendGetAsync<T>();

        T SendGet<T>();

        Task<T> SendPostAsync<T>(object body);

        T SendPost<T>(object body);

        void SendPost(object body);

        void SendPut();

        void SendPut(object body);

        T SendPut<T>(object body);
    }
}