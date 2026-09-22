using System;

namespace WpfNavigationProject.Models
{
    public class MotoDetalle
    {
// =========================================================
// DATOS DE LA MOTO
// =========================================================


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

        // =========================================================
        // DATOS DEL CLIENTE
        // =========================================================

        public string NombreCliente { get; set; } = string.Empty;
        public string DNICliente { get; set; } = string.Empty;
        public string DireccionCliente { get; set; } = string.Empty;
        public string TelefonoCliente { get; set; } = string.Empty;
    }


}
