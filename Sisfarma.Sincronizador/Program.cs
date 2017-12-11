using Sisfarma.Sincronizador.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sisfarma.Sincronizador
{
    static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var notifyIcon = new NotifyIcon();
            var app = new SincronizadorApplication();
            notifyIcon.ContextMenuStrip = GetSincronizadorMenuStrip(app);
            notifyIcon.Icon = Resources.sync;
            notifyIcon.Visible = true;            
            Application.ApplicationExit += (sender, @event) => notifyIcon.Visible = false;
            Application.Run(app); 
        }

        private static ContextMenuStrip GetSincronizadorMenuStrip(SincronizadorApplication app)
        {
            var cms = new ContextMenuStrip();
            cms.Items.Add("Salir", null, (sender, @event) =>
            {
                app.wtoken.Cancel();
                Application.Exit();
            });
            return cms;
        }
    }
}
