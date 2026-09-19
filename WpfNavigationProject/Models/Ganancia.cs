using System;

namespace WpfNavigationProject.Models
{
    public class Ganancia
    {
        public int IdGanancia { get; set; }

        public int IdServicio { get; set; }

        public decimal Monto { get; set; }

        public DateTime FechaGanancia { get; set; }
    }
}