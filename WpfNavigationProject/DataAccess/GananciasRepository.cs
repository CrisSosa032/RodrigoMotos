using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using WpfNavigationProject.Models;

namespace WpfNavigationProject.DataAccess
{
    public class GananciasRepository
    {
        // ============================================================
        // OBTENER TODAS LAS GANANCIAS
        // ============================================================

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


        // ============================================================
        // OBTENER GANANCIAS FILTRADAS POR FECHA
        // ============================================================

        public List<Ganancia> GetGananciasFiltradas(
            DateTime? fechaDesde,
            DateTime? fechaHasta)
        {
            List<Ganancia> ganancias = new List<Ganancia>();

            string sql = @"
                SELECT
                    IdGanancia,
                    IdServicio,
                    Monto,
                    FechaGanancia
                FROM Ganancias
                WHERE 1 = 1";

            if (fechaDesde.HasValue)
            {
                sql += @"
                    AND FechaGanancia >= @FechaDesde";
            }

            if (fechaHasta.HasValue)
            {
                sql += @"
                    AND FechaGanancia <= @FechaHasta";
            }

            sql += @"
                ORDER BY FechaGanancia DESC, IdGanancia DESC";


            using (SqlConnection connection = DbHelper.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                if (fechaDesde.HasValue)
                {
                    command.Parameters.AddWithValue(
                        "@FechaDesde",
                        fechaDesde.Value.Date);
                }

                if (fechaHasta.HasValue)
                {
                    command.Parameters.AddWithValue(
                        "@FechaHasta",
                        fechaHasta.Value.Date);
                }

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


        // ============================================================
        // TOTAL DE TODAS LAS GANANCIAS
        // ============================================================

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


        // ============================================================
        // TOTAL DE GANANCIAS FILTRADAS POR FECHA
        // ============================================================

        public decimal GetTotalGananciasFiltradas(
            DateTime? fechaDesde,
            DateTime? fechaHasta)
        {
            string sql = @"
                SELECT ISNULL(SUM(Monto), 0)
                FROM Ganancias
                WHERE 1 = 1";

            if (fechaDesde.HasValue)
            {
                sql += @"
                    AND FechaGanancia >= @FechaDesde";
            }

            if (fechaHasta.HasValue)
            {
                sql += @"
                    AND FechaGanancia <= @FechaHasta";
            }


            using (SqlConnection connection = DbHelper.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                if (fechaDesde.HasValue)
                {
                    command.Parameters.AddWithValue(
                        "@FechaDesde",
                        fechaDesde.Value.Date);
                }

                if (fechaHasta.HasValue)
                {
                    command.Parameters.AddWithValue(
                        "@FechaHasta",
                        fechaHasta.Value.Date);
                }

                connection.Open();

                object resultado = command.ExecuteScalar();

                return resultado != null
                    ? Convert.ToDecimal(resultado)
                    : 0m;
            }
        }


        
        // ============================================================
        // OBTENER DATOS COMPLETOS PARA REPORTE DE GANANCIAS
        // ============================================================

        public List<GananciaReporte> GetGananciasReporteFiltradas(
            DateTime? fechaDesde,
            DateTime? fechaHasta)
                {
                    List<GananciaReporte> reporte = new List<GananciaReporte>();

                    string sql = @"
                SELECT
                    G.FechaGanancia AS Fecha,
                    C.Nombre AS Cliente,
                    M.Marca,
                    M.Modelo,
                    ES.NombreEstado AS Estado,
                    S.Detalle,
                    G.Monto AS Precio

                FROM Ganancias G

                INNER JOIN Servicios S
                    ON G.IdServicio = S.IdServicio

                INNER JOIN Motos M
                    ON S.IdMoto = M.IdMoto

                INNER JOIN Clientes C
                    ON M.IdCliente = C.IdCliente

                INNER JOIN EstadosServicio ES
                    ON S.IdEstado = ES.IdEstado

                WHERE 1 = 1";

                    if (fechaDesde.HasValue)
                    {
                        sql += @"
                    AND G.FechaGanancia >= @FechaDesde";
                    }

                    if (fechaHasta.HasValue)
                    {
                        sql += @"
                    AND G.FechaGanancia <= @FechaHasta";
                    }

                    sql += @"
                ORDER BY G.FechaGanancia DESC, G.IdGanancia DESC";


                    using (SqlConnection connection = DbHelper.CreateConnection())
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        if (fechaDesde.HasValue)
                        {
                            command.Parameters.AddWithValue(
                                "@FechaDesde",
                                fechaDesde.Value.Date);
                        }

                        if (fechaHasta.HasValue)
                        {
                            command.Parameters.AddWithValue(
                                "@FechaHasta",
                                fechaHasta.Value.Date);
                        }

                        connection.Open();

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                reporte.Add(new GananciaReporte
                                {
                                    Fecha = Convert.ToDateTime(
                                        reader["Fecha"]),

                                    Cliente = reader["Cliente"]?.ToString()
                                        ?? string.Empty,

                                    Marca = reader["Marca"]?.ToString()
                                        ?? string.Empty,

                                    Modelo = reader["Modelo"]?.ToString()
                                        ?? string.Empty,

                                    Estado = reader["Estado"]?.ToString()
                                        ?? string.Empty,

                                    Detalle = reader["Detalle"]?.ToString()
                                        ?? string.Empty,

                                    Precio = Convert.ToDecimal(
                                        reader["Precio"])
                                });
                            }
                        }
                    }

                    return reporte;
        }



    }
}