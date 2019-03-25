using Sisfarma.Sincronizador.Config;
using Sisfarma.Sincronizador.Consejo;
using Sisfarma.Sincronizador.Farmatic;
using Sisfarma.Sincronizador.Fisiotes;
using Sisfarma.Sincronizador.Sincronizadores.SuperTypes;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sisfarma.Sincronizador.Sincronizadores
{
    public class SincronizadorTaskManager
    {
        public static ConcurrentBag<Task> CurrentTasks;
        public static CancellationTokenSource TokenSource;

        private static ConcurrentBag<Task> CreateConcurrentTasks()
        {
            DisposeTasks();

            TokenSource = new CancellationTokenSource();
            var cancellationToken = TokenSource.Token;

            var listaDeCompra = LocalConfig.GetSingletonInstance().ListaDeCompras;

            CurrentTasks = new ConcurrentBag<Task>
            {
                RunTask(new ClienteSincronizador(FarmaticFactory.New(), FisiotesFactory.New()), cancellationToken),

                //RunTask(new HuecoSincronizador(FarmaticFactory.New(), FisiotesFactory.New()), cancellationToken),

                //RunTask(new PuntoPendienteSincronizador(FarmaticFactory.New(), FisiotesFactory.New(), new ConsejoService()), cancellationToken),

                //RunTask(new SinonimoSincronizador(FarmaticFactory.New(), FisiotesFactory.New()), cancellationToken),

                //RunTask(new PedidoSincronizador(FarmaticFactory.New(), FisiotesFactory.New(), new ConsejoService()), cancellationToken),

                //RunTask(new ProductoCriticoSincronizador(FarmaticFactory.New(), FisiotesFactory.New(), new ConsejoService()), cancellationToken),

                //RunTask(new FamiliaSincronizador(FarmaticFactory.New(), FisiotesFactory.New()), cancellationToken),

                //RunTask(new RecetaPendienteActualizacionSincronizador(FarmaticFactory.New(), FisiotesFactory.New()), cancellationToken),

                //RunTask(new EntregaClienteActualizacionSincronizador(FarmaticFactory.New(), FisiotesFactory.New()), cancellationToken),

                //RunTask(new ProductoBorradoActualizacionSincronizador(FarmaticFactory.New(), FisiotesFactory.New()), cancellationToken),

                //RunTask(new PuntoPendienteActualizacionSincronizador(FarmaticFactory.New(), FisiotesFactory.New()), cancellationToken),

                //RunTask(new ControlSinStockSincronizador(FarmaticFactory.New(), FisiotesFactory.New(), new ConsejoService()), cancellationToken),

                //RunTask(new ControlStockSincronizador(FarmaticFactory.New(), FisiotesFactory.New(), new ConsejoService()), cancellationToken),

                //RunTask(new ControlStockFechaEntradaSincronizador(FarmaticFactory.New(), FisiotesFactory.New(), new ConsejoService()), cancellationToken),

                //RunTask(new ControlStockFechaSalidaSincronizador(FarmaticFactory.New(), FisiotesFactory.New(), new ConsejoService()), cancellationToken),

                //RunTask(new EncargoSincronizador(FarmaticFactory.New(), FisiotesFactory.New(), new ConsejoService()), cancellationToken),

                //RunTask(new CategoriaSincronizador(FarmaticFactory.New(), FisiotesFactory.New()), cancellationToken),

                //RunTask(new ListaSincronizador(FarmaticFactory.New(), FisiotesFactory.New()), cancellationToken),

                //RunTask(new ListaTiendaSincronizador(FarmaticFactory.New(), FisiotesFactory.New(), new ConsejoService(), listaDeCompra), cancellationToken),

                //RunTask(new ListaFechaSincronizador(FarmaticFactory.New(), FisiotesFactory.New(), listaDeCompra), cancellationToken),

                //RunTask(new VentaMensualActualizacionSincronizador(FarmaticFactory.New(), FisiotesFactory.New(), new ConsejoService(), listaDeCompra), cancellationToken),

                //RunTask(new EncargoActualizacionSincronizador(FarmaticFactory.New(), FisiotesFactory.New()), cancellationToken),

                //RunTask(new ProveedorHistorialSincronizador(FarmaticFactory.New(), FisiotesFactory.New()), cancellationToken),

                //RunTask(new ProveedorSincronizador(FarmaticFactory.New(), FisiotesFactory.New()), cancellationToken)
            };

            return CurrentTasks;
        }

        private static void DisposeTasks()
        {
            if (CurrentTasks == null)
                return;

            var tasks = CurrentTasks.ToArray();
            if (tasks.Any(t => t.Status == TaskStatus.Running))
            {
                TokenSource.Cancel();
                Task.WaitAll(tasks);
                foreach (var task in tasks)
                {
                    task.Dispose();
                }
            }
            TokenSource.Dispose();
            CurrentTasks = null;
        }

        public static void PowerOn()
        {
            CreateConcurrentTasks();
            Console.WriteLine("Power on success");
        }

        public static void PowerOff()
        {
            try
            {
                TokenSource.Cancel();
                Task.WaitAll(CurrentTasks.ToArray());
            }
            catch (AggregateException ex)
                when (ex.InnerExceptions.Any(inner => inner is TaskCanceledException))
            {
                var canceledTasks = ex.InnerExceptions
                    .Where(inner => inner is TaskCanceledException)
                    .Select(x => ((TaskCanceledException)x).Task);

                foreach (var t in canceledTasks)
                    t.Dispose();
            }
            finally
            {
                TokenSource.Dispose();
                CurrentTasks = null;
            }
            Console.WriteLine("Power off success");
        }

        public static Task RunTask<T>(T sincronizador, CancellationToken cancellationToken)
            where T : BaseSincronizador
            => Task.Run(() => sincronizador.SincronizarAsync(cancellationToken), cancellationToken);
    }
}