using Sisfarma.Sincronizador.Config;
using Sisfarma.Sincronizador.Fisiotes;
using Sisfarma.Sincronizador.Properties;
using Sisfarma.Sincronizador.Sincronizadores;
using System;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Deployment.Application;
using System.Reflection;
using Sisfarma.ClickOnce;
using Microsoft.Win32;
using Sisfarma.Sincronizador.Helpers;
using System.Net;

namespace Sisfarma.Sincronizador
{
    internal static class Program
    {
        private static Mutex instanceMutex;
        public static int EsUnaActualizacion = 0;

        private static void Main()
        {
            ServicePointManager.DefaultConnectionLimit = 100;

            RegisterStartup(Globals.ProductName);
            var clickOnce = new ClickOnceHelper(Globals.PublisherName, Globals.ProductName);
            clickOnce.UpdateUninstallParameters();

            bool createdNew;
            instanceMutex = new Mutex(true, @"Local\" + Assembly.GetExecutingAssembly().GetType().GUID, out createdNew);
            if (!createdNew)
            {
                Task.Delay(1000).Wait();
                if (!ApplicationDeployment.CurrentDeployment.IsFirstRun)
                {
                    instanceMutex = null;
                    return;
                }
            }
            else
            {
                Task.Delay(1000).Wait();
                if (Updater.InstallUpdateSyncWithInfo())
                    return;
            }

            string
                _remoteBase = string.Empty,
                _remoteServer = string.Empty,
                _remoteToken = string.Empty,
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
            ref _remoteToken,
            ref _localServer,
            ref _localBase,
            ref _localPass,
            ref _localUser,
            ref _marketCodeList);

            RemoteConfig.Setup(_remoteServer, _remoteToken);
            LocalConfig.Setup(_localServer, _localBase, _localUser, _localPass, _marketCodeList);

            //Task.Factory.StartNew(() => new SinonimoSincronizador(FarmaticFactory.New(), FisiotesFactory.New()).Run());
            //Task.Factory.StartNew(() => new ClienteSincronizador(FarmaticFactory.New(), FisiotesFactory.New()).Run());
            //Task.Factory.StartNew(() => new HuecoSincronizador(FarmaticFactory.New(), FisiotesFactory.New()).Run());
            //Task.Factory.StartNew(() => new PuntoPendienteSincronizador(FarmaticFactory.New(), FisiotesFactory.New(), new ConsejoService()).Run());
            //Task.Factory.StartNew(() => new PedidoSincronizador(FarmaticFactory.New(), FisiotesFactory.New(), new ConsejoService()).Run());
            //Task.Factory.StartNew(() => new ProductoCriticoSincronizador(FarmaticFactory.New(), FisiotesFactory.New(), new ConsejoService()).Run());
            //Task.Factory.StartNew(() => new FamiliaSincronizador(FarmaticFactory.New(), FisiotesFactory.New()).Run());
            //Task.Factory.StartNew(() => new RecetaPendienteActualizacionSincronizador(FarmaticFactory.New(), FisiotesFactory.New()).Run());
            //Task.Factory.StartNew(() => new EntregaClienteActualizacionSincronizador(FarmaticFactory.New(), FisiotesFactory.New()).Run());
            //Task.Factory.StartNew(() => new ProductoBorradoActualizacionSincronizador(FarmaticFactory.New(), FisiotesFactory.New()).Run());
            //Task.Factory.StartNew(() => new PuntoPendienteActualizacionSincronizador(FarmaticFactory.New(), FisiotesFactory.New()).Run());
            //Task.Factory.StartNew(() => new ControlSinStockSincronizador(FarmaticFactory.New(), FisiotesFactory.New(), new ConsejoService()).Run());
            //Task.Factory.StartNew(() => new ControlStockSincronizador(FarmaticFactory.New(), FisiotesFactory.New(), new ConsejoService()).Run());
            //Task.Factory.StartNew(() => new ControlStockFechaEntradaSincronizador(FarmaticFactory.New(), FisiotesFactory.New(), new ConsejoService()).Run());
            //Task.Factory.StartNew(() => new ControlStockFechaSalidaSincronizador(FarmaticFactory.New(), FisiotesFactory.New(), new ConsejoService()).Run());
            //Task.Factory.StartNew(() => new EncargoSincronizador(FarmaticFactory.New(), FisiotesFactory.New(), new ConsejoService()).Run());
            //Task.Factory.StartNew(() => new CategoriaSincronizador(FarmaticFactory.New(), FisiotesFactory.New()).Run());
            //Task.Factory.StartNew(() => new ListaSincronizador(FarmaticFactory.New(), FisiotesFactory.New()).Run());
            //Task.Factory.StartNew(() => new ListaTiendaSincronizador(FarmaticFactory.New(), FisiotesFactory.New(), new ConsejoService(), _marketCodeList).Run());
            //Task.Factory.StartNew(() => new ListaFechaSincronizador(FarmaticFactory.New(), FisiotesFactory.New(), _marketCodeList).Run());
            //Task.Factory.StartNew(() => new VentaMensualActualizacionSincronizador(FarmaticFactory.New(), FisiotesFactory.New(), new ConsejoService(), _marketCodeList).Run());
            //Task.Factory.StartNew(() => new EncargoActualizacionSincronizador(FarmaticFactory.New(), FisiotesFactory.New()).Run());
            //Task.Factory.StartNew(() => new ProveedorHistorialSincronizador(FarmaticFactory.New(), FisiotesFactory.New()).Run());
            //Task.Factory.StartNew(() => new ProveedorSincronizador(FarmaticFactory.New(), FisiotesFactory.New()).Run());
            Task.Factory.StartNew(() => new PowerSwitchProgramado(FisiotesFactory.New()).Run(Updater.GetCancellationToken()));
            Task.Factory.StartNew(() => new PowerSwitchManual(FisiotesFactory.New()).Run(Updater.GetCancellationToken()));
            Task.Factory.StartNew(() => new UpdateVersionSincronizador().Run(new CancellationToken()));

            Application.ApplicationExit += (sender, @event) => notifyIcon.Visible = false;
            Application.Run(new SincronizadorApplication());

            instanceMutex.ReleaseMutex();
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
            ref string _remoteToken,
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
                _remoteToken = stream.ReadLine();

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

        internal static void RegisterStartup(string productName)
        {
            if (!ApplicationDeployment.IsNetworkDeployed)
                return;
            RegistryKey reg = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            reg.SetValue(productName, Assembly.GetExecutingAssembly().Location);
        }
    }
}