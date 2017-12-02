namespace Sisfarma.Sincronizador.Fisiotes.Models
{
    public partial class Configuracion
    {
        public ulong id { get; set; }
   
        public string campo { get; set; }

        public string valor { get; set; }

        public bool? activo { get; set; }
    }
}
