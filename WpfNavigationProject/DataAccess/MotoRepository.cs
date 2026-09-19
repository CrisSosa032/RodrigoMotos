using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using WpfNavigationProject.Models;

namespace WpfNavigationProject.DataAccess
{
    public class MotoRepository
    {
        // ============================================================
        // OBTENER TODAS LAS MOTOS
        // ============================================================

        public List<Moto> GetAllMotos()
        {
            List<Moto> motos = new List<Moto>();

            string sql = @"
                SELECT 
                    m.IdMoto,
                    m.IdCliente,
                    m.Marca,
                    m.Modelo,
                    m.Anio,
                    m.Patente,
                    m.NroMotor,
                    m.NroChasis,
                    m.Observaciones,
                    m.FechaAlta
                FROM Motos m
                INNER JOIN Clientes c
                    ON m.IdCliente = c.IdCliente
                ORDER BY m.IdMoto";

            using (SqlConnection connection =
                   DbHelper.CreateConnection())

            using (SqlCommand command =
                   new SqlCommand(sql, connection))
            {
                try
                {
                    connection.Open();

                    using (SqlDataReader reader =
                           command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            motos.Add(MapearMoto(reader));
                        }
                    }
                }
                catch (SqlException ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        "Error SQL: " + ex.Message);

                    throw;
                }
            }

            return motos;
        }


        // ============================================================
        // OBTENER MOTOS FILTRADAS
        // ============================================================

        public List<Moto> GetMotosFiltradas(
            string texto,
            string campoBusqueda,
            DateTime? fechaDesde,
            DateTime? fechaHasta)
        {
            List<Moto> motos = new List<Moto>();

            string sql = @"
                SELECT
                    m.IdMoto,
                    m.IdCliente,
                    m.Marca,
                    m.Modelo,
                    m.Anio,
                    m.Patente,
                    m.NroMotor,
                    m.NroChasis,
                    m.Observaciones,
                    m.FechaAlta
                FROM Motos m
                INNER JOIN Clientes c
                    ON m.IdCliente = c.IdCliente
                WHERE
                    (
                        @Texto = ''
                        OR
                        (
                            @Campo = 'Marca'
                            AND m.Marca LIKE '%' + @Texto + '%'
                        )
                        OR
                        (
                            @Campo = 'Modelo'
                            AND m.Modelo LIKE '%' + @Texto + '%'
                        )
                        OR
                        (
                            @Campo = 'Patente'
                            AND m.Patente LIKE '%' + @Texto + '%'
                        )
                        OR
                        (
                            @Campo = 'Cliente'
                            AND c.Nombre LIKE '%' + @Texto + '%'
                        )
                    )

                    AND
                    (
                        @FechaDesde IS NULL
                        OR m.FechaAlta >= @FechaDesde
                    )

                    AND
                    (
                        @FechaHasta IS NULL
                        OR m.FechaAlta <= @FechaHasta
                    )

                ORDER BY
                    m.Marca,
                    m.Modelo,
                    m.IdMoto";

            using (SqlConnection connection =
                   DbHelper.CreateConnection())

            using (SqlCommand command =
                   new SqlCommand(sql, connection))
            {
                // ====================================================
                // TEXTO Y CAMPO DE BÚSQUEDA
                // ====================================================

                command.Parameters.AddWithValue(
                    "@Texto",
                    texto ?? string.Empty);

                command.Parameters.AddWithValue(
                    "@Campo",
                    campoBusqueda ?? "Marca");


                // ====================================================
                // FILTRO FECHA DESDE
                // ====================================================

                if (fechaDesde.HasValue)
                {
                    command.Parameters.Add(
                        "@FechaDesde",
                        System.Data.SqlDbType.Date)
                        .Value = fechaDesde.Value.Date;
                }
                else
                {
                    command.Parameters.Add(
                        "@FechaDesde",
                        System.Data.SqlDbType.Date)
                        .Value = DBNull.Value;
                }


                // ====================================================
                // FILTRO FECHA HASTA
                // ====================================================

                if (fechaHasta.HasValue)
                {
                    command.Parameters.Add(
                        "@FechaHasta",
                        System.Data.SqlDbType.Date)
                        .Value = fechaHasta.Value.Date;
                }
                else
                {
                    command.Parameters.Add(
                        "@FechaHasta",
                        System.Data.SqlDbType.Date)
                        .Value = DBNull.Value;
                }


                // ====================================================
                // EJECUTAR CONSULTA
                // ====================================================

                connection.Open();

                using (SqlDataReader reader =
                       command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        motos.Add(MapearMoto(reader));
                    }
                }
            }

            return motos;
        }


        // ============================================================
        // OBTENER UNA MOTO POR ID
        // ============================================================

        public Moto? GetMotoById(int idMoto)
        {
            string sql = @"
                SELECT
                    m.IdMoto,
                    m.IdCliente,
                    m.Marca,
                    m.Modelo,
                    m.Anio,
                    m.Patente,
                    m.NroMotor,
                    m.NroChasis,
                    m.Observaciones,
                    m.FechaAlta
                FROM Motos m
                INNER JOIN Clientes c
                    ON m.IdCliente = c.IdCliente
                WHERE m.IdMoto = @Id";

            using (SqlConnection connection =
                   DbHelper.CreateConnection())

            using (SqlCommand command =
                   new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue(
                    "@Id",
                    idMoto);

                connection.Open();

                using (SqlDataReader reader =
                       command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapearMoto(reader);
                    }
                }
            }

            return null;
        }


        // ============================================================
        // INSERTAR MOTO
        // ============================================================

        public int AddMoto(Moto moto)
        {
            string sql = @"
                INSERT INTO Motos
                (
                    IdCliente,
                    Marca,
                    Modelo,
                    Anio,
                    Patente,
                    NroMotor,
                    NroChasis,
                    Observaciones
                )
                VALUES
                (
                    @IdCliente,
                    @Marca,
                    @Modelo,
                    @Anio,
                    @Patente,
                    @NroMotor,
                    @NroChasis,
                    @Observaciones
                );

                SELECT SCOPE_IDENTITY();";

            using (SqlConnection connection =
                   DbHelper.CreateConnection())

            using (SqlCommand command =
                   new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue(
                    "@IdCliente",
                    moto.IdCliente);

                command.Parameters.AddWithValue(
                    "@Marca",
                    (object)moto.Marca ?? DBNull.Value);

                command.Parameters.AddWithValue(
                    "@Modelo",
                    (object)moto.Modelo ?? DBNull.Value);

                command.Parameters.AddWithValue(
                    "@Anio",
                    moto.Anio);

                command.Parameters.AddWithValue(
                    "@Patente",
                    (object)moto.Patente ?? DBNull.Value);

                command.Parameters.AddWithValue(
                    "@NroMotor",
                    (object)moto.NroMotor ?? DBNull.Value);

                command.Parameters.AddWithValue(
                    "@NroChasis",
                    (object)moto.NroChasis ?? DBNull.Value);

                command.Parameters.AddWithValue(
                    "@Observaciones",
                    (object)moto.Observaciones ?? DBNull.Value);

                try
                {
                    connection.Open();

                    object result =
                        command.ExecuteScalar();

                    return result != null
                        ? Convert.ToInt32(result)
                        : 0;
                }
                catch (SqlException ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"Error SQL al insertar moto: {ex.Message}");

                    throw;
                }
            }
        }


        // ============================================================
        // ACTUALIZAR MOTO
        // ============================================================

        public void UpdateMoto(Moto moto)
        {
            string sql = @"
                UPDATE Motos
                SET
                    IdCliente = @IdCliente,
                    Marca = @Marca,
                    Modelo = @Modelo,
                    Anio = @Anio,
                    Patente = @Patente,
                    NroMotor = @NroMotor,
                    NroChasis = @NroChasis,
                    Observaciones = @Observaciones
                WHERE IdMoto = @Id";

            using (SqlConnection connection =
                   DbHelper.CreateConnection())

            using (SqlCommand command =
                   new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue(
                    "@Id",
                    moto.IdMoto);

                command.Parameters.AddWithValue(
                    "@IdCliente",
                    moto.IdCliente);

                command.Parameters.AddWithValue(
                    "@Marca",
                    (object)moto.Marca ?? DBNull.Value);

                command.Parameters.AddWithValue(
                    "@Modelo",
                    (object)moto.Modelo ?? DBNull.Value);

                command.Parameters.AddWithValue(
                    "@Anio",
                    moto.Anio);

                command.Parameters.AddWithValue(
                    "@Patente",
                    (object)moto.Patente ?? DBNull.Value);

                command.Parameters.AddWithValue(
                    "@NroMotor",
                    (object)moto.NroMotor ?? DBNull.Value);

                command.Parameters.AddWithValue(
                    "@NroChasis",
                    (object)moto.NroChasis ?? DBNull.Value);

                command.Parameters.AddWithValue(
                    "@Observaciones",
                    (object)moto.Observaciones ?? DBNull.Value);

                connection.Open();

                command.ExecuteNonQuery();
            }
        }


        // ============================================================
        // VERIFICAR SI EL CLIENTE DE LA MOTO ESTÁ ACTIVO
        // ============================================================

        public bool ClienteEstaActivo(int idMoto)
        {
            string sql = @"
                SELECT c.Activo
                FROM Motos m
                INNER JOIN Clientes c
                    ON m.IdCliente = c.IdCliente
                WHERE m.IdMoto = @IdMoto";

            using (SqlConnection connection =
                   DbHelper.CreateConnection())

            using (SqlCommand command =
                   new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue(
                    "@IdMoto",
                    idMoto);

                connection.Open();

                object resultado =
                    command.ExecuteScalar();

                if (resultado == null)
                {
                    throw new Exception(
                        "No se encontró el cliente dueño de esta moto.");
                }

                return Convert.ToBoolean(resultado);
            }
        }


        // ============================================================
        // OBTENER ID CLIENTE DE UNA MOTO
        // ============================================================

        public int GetIdClienteByMoto(int idMoto)
        {
            string sql = @"
                SELECT IdCliente
                FROM Motos
                WHERE IdMoto = @IdMoto";

            using (SqlConnection connection =
                   DbHelper.CreateConnection())

            using (SqlCommand command =
                   new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue(
                    "@IdMoto",
                    idMoto);

                connection.Open();

                object resultado =
                    command.ExecuteScalar();

                if (resultado == null)
                {
                    throw new Exception(
                        "No se encontró la moto.");
                }

                return Convert.ToInt32(resultado);
            }
        }


        // ============================================================
        // MAPEAR MOTO
        // ============================================================

        /// <summary>
        /// Convierte una fila de SQL en un objeto Moto.
        /// Centralizar esto evita repetir el mismo código
        /// en todos los métodos del repositorio.
        /// </summary>
        private Moto MapearMoto(SqlDataReader reader)
        {
            return new Moto
            {
                IdMoto = reader.GetInt32(
                    reader.GetOrdinal("IdMoto")),

                IdCliente = reader.GetInt32(
                    reader.GetOrdinal("IdCliente")),

                Marca = reader["Marca"] != DBNull.Value
                    ? reader["Marca"].ToString()!
                    : string.Empty,

                Modelo = reader["Modelo"] != DBNull.Value
                    ? reader["Modelo"].ToString()!
                    : string.Empty,

                Anio = reader["Anio"] != DBNull.Value
                    ? (short)Convert.ToInt32(reader["Anio"])
                    : (short)0,

                Patente = reader["Patente"] != DBNull.Value
                    ? reader["Patente"].ToString()!
                    : string.Empty,

                NroMotor = reader["NroMotor"] != DBNull.Value
                    ? reader["NroMotor"].ToString()!
                    : string.Empty,

                NroChasis = reader["NroChasis"] != DBNull.Value
                    ? reader["NroChasis"].ToString()!
                    : string.Empty,

                Observaciones = reader["Observaciones"] != DBNull.Value
                    ? reader["Observaciones"].ToString()!
                    : string.Empty,

                FechaAlta = reader["FechaAlta"] != DBNull.Value
                    ? Convert.ToDateTime(reader["FechaAlta"])
                    : DateTime.MinValue
            };
        }
    }
}
