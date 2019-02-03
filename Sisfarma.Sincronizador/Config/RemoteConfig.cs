using System;

namespace Sisfarma.Sincronizador.Config
{
    public class RemoteConfig : UserConfig
    {
        private static RemoteConfig _singleton = null;
        
        public string Server { get; private set; }

        private RemoteConfig(string server, string username, string password)
            : base(username, password) 
            => Server = server ?? throw new ArgumentNullException(nameof(server));                      

        public static void Setup(string server, string username, string password)
        {
            _singleton = null;
            _singleton = new RemoteConfig(server, username, password);                        
        }

        public static RemoteConfig GetSingletonInstance()
        {
            if (_singleton == null)
                throw new InvalidOperationException(nameof(RemoteConfig));

            return _singleton;
        }
    }
}
