using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace WpfNavigationProject.Models
{
    public class Cliente
    {
        // Las propiedades deben coincidir con los nombres de las columnas en SQL
        public int IdCliente { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string DNI { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public DateTime FechaIngreso { get; set; }
        public DateTime? FechaBaja { get; set; }
    }
}