using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using WpfNavigationProject.Models;

namespace WpfNavigationProject.DataAccess
{
    public class GananciasRepository
    {
        /// <summary>
        /// Obtiene todas las ganancias registradas.
        /// </summary>
        public List<Ganancia> GetAllGanancias()
        {
            List<Ganancia> ganancias = new List<Ganancia>();

            string sql = @"
                SELECT
                    IdGanancia,
                    IdServicio,
                    Monto,
                    FechaGanancia
                FROM Ganancias
                ORDER BY FechaGanancia DESC, IdGanancia DESC";

            using (SqlConnection connection = DbHelper.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ganancias.Add(new Ganancia
                        {
                            IdGanancia = Convert.ToInt32(
                                reader["IdGanancia"]),

                            IdServicio = Convert.ToInt32(
                                reader["IdServicio"]),

                            Monto = Convert.ToDecimal(
                                reader["Monto"]),

                            FechaGanancia = Convert.ToDateTime(
                                reader["FechaGanancia"])
                        });
                    }
                }
            }

            return ganancias;
        }

        /// <summary>
        /// Obtiene el total de todas las ganancias.
        /// </summary>
        public decimal GetTotalGanancias()
        {
            string sql = @"
                SELECT ISNULL(SUM(Monto), 0)
                FROM Ganancias";

            using (SqlConnection connection = DbHelper.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                connection.Open();

                object resultado = command.ExecuteScalar();

                return resultado != null
                    ? Convert.ToDecimal(resultado)
                    : 0m;
            }
        }
    }
}