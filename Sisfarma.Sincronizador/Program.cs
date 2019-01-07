using Sisfarma.Sincronizador.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sisfarma.Sincronizador
{
    internal static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            var notifyIcon = new NotifyIcon();
            notifyIcon.ContextMenuStrip = GetSincronizadorMenuStrip();
            notifyIcon.Icon = Resources.sync;
            notifyIcon.Visible = true;

            Application.ApplicationExit += (sender, @event) => notifyIcon.Visible = false;
            Application.Run(new SincronizadorApplication());
        }

        private static ContextMenuStrip GetSincronizadorMenuStrip()
        {
            var cms = new ContextMenuStrip();
            cms.Items.Add("Salir", null, (sender, @event) => Application.Exit());
            return cms;
        }
    }
}