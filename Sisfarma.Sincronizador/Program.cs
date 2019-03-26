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

            Task.Factory.StartNew(() => new PowerSwitchProgramado(FisiotesFactory.New()).SincronizarAsync(Updater.GetCancellationToken(), delayLoop: 60000));
            Task.Factory.StartNew(() => new PowerSwitchManual(FisiotesFactory.New()).SincronizarAsync(Updater.GetCancellationToken(), delayLoop: 60000));
            Task.Factory.StartNew(() => new UpdateVersionSincronizador().SincronizarAsync(new CancellationToken(), delayLoop: 60000));

            Application.ApplicationExit += (sender, @event) => notifyIcon.Visible = false;
            Application.Run(new SincronizadorApplication());

            instanceMutex.ReleaseMutex();
        }

        private static ContextMenuStrip GetSincronizadorMenuStrip()
        {
            var cms = new ContextMenuStrip();
            //cms.Items.Add($"Salir {ApplicationDeployment.CurrentDeployment.CurrentVersion}", null, (sender, @event) => Application.Exit());
            cms.Items.Add($"Salir", null, (sender, @event) => Application.Exit());
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