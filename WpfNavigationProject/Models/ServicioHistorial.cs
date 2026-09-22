using System;

namespace WpfNavigationProject.Models
{
    public class ServicioHistorial
    {
        public int IdHistorial { get; set; }

        public int IdServicio { get; set; }

        public int IdEstado { get; set; }

        public DateTime FechaCambio { get; set; }

        public string NombreEstado { get; set; } = string.Empty;
    }
}