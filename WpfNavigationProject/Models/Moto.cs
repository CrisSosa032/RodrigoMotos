using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfNavigationProject.Models
{
    public class Moto
    {
        public int IdMoto { get; set; }
        public int IdCliente { get; set; }
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public short? Anio { get; set; }
        public string Patente { get; set; } = string.Empty;
        public string NroMotor { get; set; } = string.Empty;
        public string NroChasis { get; set; } = string.Empty;
        public string Observaciones { get; set; } = string.Empty;
        public DateTime FechaAlta { get; set; }
    }
}