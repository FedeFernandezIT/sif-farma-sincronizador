using System.Threading.Tasks;

namespace Sisfarma.Sincronizador.Sincronizadores.SuperTypes
{
    public interface ISincronizador
    {
        Task Run();
    }
}
