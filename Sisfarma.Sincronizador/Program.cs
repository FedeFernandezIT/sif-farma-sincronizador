using Sisfarma.Sincronizador.Consejo;
using Sisfarma.Sincronizador.Farmatic;
using Sisfarma.Sincronizador.Fisiotes;
using Sisfarma.Sincronizador.Properties;
using Sisfarma.Sincronizador.Sincronizadores;
using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sisfarma.Sincronizador
{
    internal static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        //[STAThread]
        private static void Main()
        {
            string
                _remoteBase = string.Empty,
                _remoteServer = string.Empty,
                _remoteUsername = string.Empty,
                _remotePassword = string.Empty,
                _localBase = string.Empty,
                _localServer = string.Empty,
                _localUser = string.Empty,
                _localPass = string.Empty;

            int _marketCodeList = 0;

            var notifyIcon = new NotifyIcon();
            notifyIcon.ContextMenuStrip = GetSincronizadorMenuStrip();
            notifyIcon.Icon = Resources.sync;
            notifyIcon.Visible = true;


            LeerFicherosConfiguracion(
            ref _remoteBase,
            ref _remoteServer,
            ref _remoteUsername,
            ref _remotePassword,
            ref _localServer,
            ref _localBase,
            ref _localPass,
            ref _localUser,
            ref _marketCodeList);


            ConsejoService consejoService = new ConsejoService();

            Task.Factory.StartNew(() => new ClienteSincronizador(NewFarmatic(), NewFisiotes()).Run());
            Task.Factory.StartNew(() => new HuecoSincronizador(NewFarmatic(), NewFisiotes()).Run());
            Task.Factory.StartNew(() => new PuntosPendientesSincronizador(NewFarmatic(), NewFisiotes(), consejoService).Run());
            Task.Factory.StartNew(() => new SinonimoSincronizador(NewFarmatic(), NewFisiotes()).Run());
            Task.Factory.StartNew(() => new PedidoSincronizador(NewFarmatic(), NewFisiotes(), consejoService).Run());
            Task.Factory.StartNew(() => new ProductoCriticoSincronizador(NewFarmatic(), NewFisiotes(), consejoService).Run());


            Application.ApplicationExit += (sender, @event) => notifyIcon.Visible = false;            
            Application.Run(new SincronizadorApplication());

            

            FisiotesService NewFisiotes()            
                => new FisiotesService(_remoteServer, _remoteUsername, _remoteUsername);
            
            FarmaticService NewFarmatic()
                => new FarmaticService(_localServer, _localBase, _localUser, _localPass);                        
        }

        private static ContextMenuStrip GetSincronizadorMenuStrip()
        {
            var cms = new ContextMenuStrip();
            cms.Items.Add("Salir", null, (sender, @event) => Application.Exit());
            return cms;
        }

        private static void LeerFicherosConfiguracion(
            ref string _remoteBase,
            ref string _remoteServer,
            ref string _remoteUsername,
            ref string _remotePassword,
            ref string _localServer,
            ref string _localBase,
            ref string _localPass,
            ref string _localUser,
            ref int _marketCodeList)
        {
            try
            {
                var dir = ConfigurationManager.AppSettings["Directory.Setup"];

                // Configuramos el acceso a el servidor remoto de MySql
                var path = ConfigurationManager.AppSettings["File.Remote.Base"];
                _remoteBase = new StreamReader(Path.Combine(dir, path)).ReadLine();

                path = ConfigurationManager.AppSettings["File.Remote.Server"];
                var stream = new StreamReader(Path.Combine(dir, path));
                _remoteServer = stream.ReadLine();
                _remoteUsername = stream.ReadLine();
                _remotePassword = stream.ReadLine();

                // Servidor Local
                path = ConfigurationManager.AppSettings["File.Local.Server"];
                _localServer = new StreamReader(Path.Combine(dir, path)).ReadLine();
                path = ConfigurationManager.AppSettings["File.Local.Base"];
                _localBase = new StreamReader(Path.Combine(dir, path)).ReadLine();
                path = ConfigurationManager.AppSettings["File.Local.User"];
                _localUser = new StreamReader(Path.Combine(dir, path)).ReadLine();
                path = ConfigurationManager.AppSettings["File.Local.Pass"];
                _localPass = new StreamReader(Path.Combine(dir, path)).ReadLine();

                // Único archivo que puede no existir
                path = ConfigurationManager.AppSettings["File.Market.Code.List"];
                _marketCodeList = File.Exists(Path.Combine(dir, path))
                    ? Convert.ToInt32(new StreamReader(Path.Combine(dir, path)).ReadLine())
                    : -1;
            }
            catch (IOException)
            {
                throw new IOException("Ha habido un error en la lectura de algún fichero de configuración. Compruebe que existen dichos ficheros de configuración.");
            }
        }        
    }
}