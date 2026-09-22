using System;

namespace WpfNavigationProject.Models
{
    public class GananciaReporte
    {
        public DateTime Fecha { get; set; }

        public string Cliente { get; set; } = string.Empty;

        public string Marca { get; set; } = string.Empty;

        public string Modelo { get; set; } = string.Empty;

        public string Estado { get; set; } = string.Empty;

        public string Detalle { get; set; } = string.Empty;

        public decimal Precio { get; set; }
    }
}
