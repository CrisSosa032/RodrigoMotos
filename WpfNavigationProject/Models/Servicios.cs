using System;
using Microsoft.Data.SqlClient;
using static Azure.Core.HttpHeader;
using System.Data;
using System.Diagnostics;
using System.Windows.Controls;
using WpfNavigationProject.DataAccess;
using WpfNavigationProject.Models;

namespace WpfNavigationProject.Models
{
    public class Servicios
    {
        // ============================================================
        // IDENTIFICADORES INTERNOS
        // ============================================================

        public int IdServicio { get; set; }

        public int IdMoto { get; set; }

        public int IdEstado { get; set; }

        // ============================================================
        // DATOS DEL SERVICIO
        // ============================================================

        public string Detalle { get; set; } = string.Empty;

        public DateTime FechaEntrada { get; set; }

        public decimal? CostoEstimado { get; set; }

        // ============================================================
        // DATOS VISUALES / RELACIONES
        // ============================================================

        public string NombreCliente { get; set; } = string.Empty;

        public string MarcaMoto { get; set; } = string.Empty;

        public string ModeloMoto { get; set; } = string.Empty;

        public string NombreEstado { get; set; } = string.Empty;
    }
}