namespace WpfNavigationProject.Models
{
    public class TrabajoEnCurso
    {
        public int IdServicio { get; set; }

        public int IdMoto { get; set; }

        public int IdEstado { get; set; }

        public string Detalle { get; set; } = string.Empty;

        public DateTime FechaEntrada { get; set; }

        public string Marca { get; set; } = string.Empty;

        public string Modelo { get; set; } = string.Empty;

        public string Patente { get; set; } = string.Empty;

        public string Cliente { get; set; } = string.Empty;

        public string MotoDescripcion
        {
            get
            {
                return $"{Marca} {Modelo}".Trim();
            }
        }
    }
}