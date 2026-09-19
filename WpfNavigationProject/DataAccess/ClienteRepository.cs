using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using WpfNavigationProject.Models;

namespace WpfNavigationProject.DataAccess
{
    public class ClienteRepository
    {
        // --------------------------------------------------------
        // OBTENER CLIENTES ACTIVOS
        // --------------------------------------------------------

        public List<Cliente> GetAllClientes()
        {
            List<Cliente> clientes = new List<Cliente>();

            string sql = @"
                SELECT 
                    IdCliente, 
                    Nombre, 
                    DNI, 
                    Direccion, 
                    Telefono, 
                    Activo, 
                    FechaIngreso, 
                    FechaBaja
                FROM Clientes
                WHERE Activo = 1
                ORDER BY Nombre";

            using (SqlConnection connection = DbHelper.CreateConnection())
            {
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    try
                    {
                        connection.Open();

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                clientes.Add(new Cliente
                                {
                                    IdCliente = Convert.ToInt32(
                                        reader["IdCliente"]),

                                    Nombre = reader["Nombre"].ToString()
                                        ?? string.Empty,

                                    DNI = reader["DNI"].ToString()
                                        ?? string.Empty,

                                    Direccion = reader["Direccion"].ToString()
                                        ?? string.Empty,

                                    Telefono = reader["Telefono"].ToString()
                                        ?? string.Empty,

                                    Activo = reader["Activo"] != DBNull.Value
                                        && Convert.ToBoolean(
                                            reader["Activo"]),

                                    FechaIngreso = Convert.ToDateTime(
                                        reader["FechaIngreso"]),

                                    FechaBaja = reader["FechaBaja"] != DBNull.Value
                                        ? Convert.ToDateTime(
                                            reader["FechaBaja"])
                                        : null
                                });
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
            }

            return clientes;
        }


        // --------------------------------------------------------
        // BUSCADOR Y FILTROS
        // --------------------------------------------------------

        /// <summary>
        /// Obtiene clientes aplicando los filtros del buscador.
        /// 
        /// activo:
        /// true  = solamente activos
        /// false = solamente inactivos
        /// null  = todos
        /// 
        /// buscarPor:
        /// "Nombre" o "DNI"
        /// 
        /// fechaDesde y fechaHasta filtran FechaIngreso.
        /// </summary>
        public List<Cliente> GetClientesFiltrados(
            string texto,
            string buscarPor,
            bool? activo,
            DateTime? fechaDesde,
            DateTime? fechaHasta)
        {
            List<Cliente> clientes = new List<Cliente>();

            string sql = @"
                SELECT 
                    IdCliente,
                    Nombre,
                    DNI,
                    Direccion,
                    Telefono,
                    Activo,
                    FechaIngreso,
                    FechaBaja
                FROM Clientes
                WHERE
                    (
                        @Texto = ''
                        OR
                        (
                            @BuscarPor = 'Nombre'
                            AND Nombre LIKE '%' + @Texto + '%'
                        )
                        OR
                        (
                            @BuscarPor = 'DNI'
                            AND DNI LIKE '%' + @Texto + '%'
                        )
                    )
                    AND
                    (
                        @Activo IS NULL
                        OR Activo = @Activo
                    )
                    AND
                    (
                        @FechaDesde IS NULL
                        OR FechaIngreso >= @FechaDesde
                    )
                    AND
                    (
                        @FechaHasta IS NULL
                        OR FechaIngreso <= @FechaHasta
                    )
                ORDER BY Nombre";

            using (SqlConnection connection = DbHelper.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue(
                    "@Texto",
                    texto ?? string.Empty);

                command.Parameters.AddWithValue(
                    "@BuscarPor",
                    buscarPor);

                command.Parameters.Add(
                    new SqlParameter("@Activo", System.Data.SqlDbType.Bit)
                    {
                        Value = activo.HasValue
                            ? (object)activo.Value
                            : DBNull.Value
                    });

                command.Parameters.Add(
                    new SqlParameter("@FechaDesde", System.Data.SqlDbType.Date)
                    {
                        Value = fechaDesde.HasValue
                            ? (object)fechaDesde.Value.Date
                            : DBNull.Value
                    });

                command.Parameters.Add(
                    new SqlParameter("@FechaHasta", System.Data.SqlDbType.Date)
                    {
                        Value = fechaHasta.HasValue
                            ? (object)fechaHasta.Value.Date
                            : DBNull.Value
                    });

                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        clientes.Add(new Cliente
                        {
                            IdCliente = Convert.ToInt32(
                                reader["IdCliente"]),

                            Nombre = reader["Nombre"].ToString()
                                ?? string.Empty,

                            DNI = reader["DNI"].ToString()
                                ?? string.Empty,

                            Direccion = reader["Direccion"].ToString()
                                ?? string.Empty,

                            Telefono = reader["Telefono"].ToString()
                                ?? string.Empty,

                            Activo = reader["Activo"] != DBNull.Value
                                && Convert.ToBoolean(
                                    reader["Activo"]),

                            FechaIngreso = Convert.ToDateTime(
                                reader["FechaIngreso"]),

                            FechaBaja = reader["FechaBaja"] != DBNull.Value
                                ? Convert.ToDateTime(
                                    reader["FechaBaja"])
                                : null
                        });
                    }
                }
            }

            return clientes;
        }


        // --------------------------------------------------------
        // AGREGAR CLIENTE
        // --------------------------------------------------------

        public int AddCliente(Cliente cliente)
        {
            string sql = @"
                INSERT INTO Clientes
                (
                    Nombre,
                    DNI,
                    Direccion,
                    Telefono,
                    Activo,
                    FechaIngreso,
                    FechaBaja
                )
                VALUES
                (
                    @Nombre,
                    @DNI,
                    @Direccion,
                    @Telefono,
                    1,
                    CAST(GETDATE() AS DATE),
                    NULL
                );

                SELECT SCOPE_IDENTITY();";

            using (SqlConnection connection = DbHelper.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue(
                    "@Nombre",
                    cliente.Nombre);

                command.Parameters.AddWithValue(
                    "@DNI",
                    cliente.DNI);

                command.Parameters.AddWithValue(
                    "@Direccion",
                    (object)cliente.Direccion ?? DBNull.Value);

                command.Parameters.AddWithValue(
                    "@Telefono",
                    (object)cliente.Telefono ?? DBNull.Value);

                try
                {
                    connection.Open();

                    object result = command.ExecuteScalar();

                    return result != null
                        ? Convert.ToInt32(result)
                        : 0;
                }
                catch (SqlException ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"Error SQL al insertar cliente: {ex.Message}");

                    throw;
                }
            }
        }


        // --------------------------------------------------------
        // OBTENER CLIENTE POR ID
        // --------------------------------------------------------

        public Cliente? GetClienteById(int id)
        {
            string sql = @"
                SELECT
                    IdCliente,
                    Nombre,
                    DNI,
                    Telefono,
                    Direccion,
                    Activo,
                    FechaIngreso,
                    FechaBaja
                FROM Clientes
                WHERE IdCliente = @Id";

            using (SqlConnection connection = DbHelper.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@Id", id);

                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Cliente
                        {
                            IdCliente = Convert.ToInt32(
                                reader["IdCliente"]),

                            Nombre = reader["Nombre"].ToString()
                                ?? string.Empty,

                            DNI = reader["DNI"].ToString()
                                ?? string.Empty,

                            Telefono = reader["Telefono"].ToString()
                                ?? string.Empty,

                            Direccion = reader["Direccion"].ToString()
                                ?? string.Empty,

                            Activo = reader["Activo"] != DBNull.Value
                                && Convert.ToBoolean(
                                    reader["Activo"]),

                            FechaIngreso = Convert.ToDateTime(
                                reader["FechaIngreso"]),

                            FechaBaja = reader["FechaBaja"] != DBNull.Value
                                ? Convert.ToDateTime(
                                    reader["FechaBaja"])
                                : null
                        };
                    }
                }
            }

            return null;
        }


        // --------------------------------------------------------
        // ACTUALIZAR CLIENTE
        // --------------------------------------------------------

        public void UpdateCliente(Cliente cliente)
        {
            string sql = @"
                UPDATE Clientes
                SET
                    Nombre = @Nombre,
                    DNI = @DNI,
                    Telefono = @Telefono,
                    Direccion = @Direccion
                WHERE IdCliente = @Id";

            using (SqlConnection connection = DbHelper.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue(
                    "@Nombre",
                    cliente.Nombre);

                command.Parameters.AddWithValue(
                    "@DNI",
                    cliente.DNI);

                command.Parameters.AddWithValue(
                    "@Telefono",
                    (object)cliente.Telefono ?? DBNull.Value);

                command.Parameters.AddWithValue(
                    "@Direccion",
                    (object)cliente.Direccion ?? DBNull.Value);

                command.Parameters.AddWithValue(
                    "@Id",
                    cliente.IdCliente);

                connection.Open();

                command.ExecuteNonQuery();
            }
        }


        // --------------------------------------------------------
        // SOFT DELETE DE CLIENTE
        // --------------------------------------------------------

        public void DeleteCliente(int id)
        {
            string sql = @"
                UPDATE Clientes
                SET
                    Activo = 0,
                    FechaBaja = CAST(GETDATE() AS DATE)
                WHERE IdCliente = @Id;";

            using (SqlConnection connection = DbHelper.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@Id", id);

                connection.Open();

                command.ExecuteNonQuery();
            }
        }


        // --------------------------------------------------------
        // OBTENER CLIENTES INACTIVOS
        // --------------------------------------------------------

        public List<Cliente> GetClientesEliminados()
        {
            List<Cliente> clientesList =
                new List<Cliente>();

            string sql = @"
                SELECT
                    IdCliente,
                    Nombre,
                    DNI,
                    Direccion,
                    Telefono,
                    Activo,
                    FechaIngreso,
                    FechaBaja
                FROM Clientes
                WHERE Activo = 0
                ORDER BY Nombre";

            using (SqlConnection connection = DbHelper.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                try
                {
                    connection.Open();

                    using (SqlDataReader reader =
                        command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            clientesList.Add(new Cliente
                            {
                                IdCliente = Convert.ToInt32(
                                    reader["IdCliente"]),

                                Nombre = reader["Nombre"].ToString()
                                    ?? string.Empty,

                                DNI = reader["DNI"].ToString()
                                    ?? string.Empty,

                                Direccion = reader["Direccion"].ToString()
                                    ?? string.Empty,

                                Telefono = reader["Telefono"].ToString()
                                    ?? string.Empty,

                                Activo = reader["Activo"] != DBNull.Value
                                    && Convert.ToBoolean(
                                        reader["Activo"]),

                                FechaIngreso = Convert.ToDateTime(
                                    reader["FechaIngreso"]),

                                FechaBaja = reader["FechaBaja"] != DBNull.Value
                                    ? Convert.ToDateTime(
                                        reader["FechaBaja"])
                                    : null
                            });
                        }
                    }
                }
                catch (SqlException ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        "Error SQL al obtener clientes eliminados: "
                        + ex.Message);

                    throw;
                }
            }

            return clientesList;
        }


        // --------------------------------------------------------
        // RESTAURAR CLIENTE
        // --------------------------------------------------------

        public void RestaurarCliente(int id)
        {
            string sql = @"
                UPDATE Clientes
                SET
                    Activo = 1,
                    FechaIngreso = CAST(GETDATE() AS DATE),
                    FechaBaja = NULL
                WHERE IdCliente = @Id;";

            using (SqlConnection connection = DbHelper.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@Id", id);

                connection.Open();

                command.ExecuteNonQuery();
            }
        }



        // --------------------------------------------------------
        // DESACTIVACIÓN AUTOMÁTICA DE CLIENTES INACTIVOS
        // --------------------------------------------------------

        public void DesactivarClientesInactivos()
        {
            string sql = @"
                        UPDATE Clientes
                        SET
                            Activo = 0,
                            FechaBaja = CAST(GETDATE() AS DATE)
                        WHERE
                            Activo = 1
                            AND IdCliente IN
                            (
                                SELECT m.IdCliente
                                FROM Motos m
                                INNER JOIN Servicios s
                                    ON s.IdMoto = m.IdMoto
                                GROUP BY m.IdCliente
                                HAVING MAX(s.FechaEntrada) < DATEADD(MONTH, -3, GETDATE())
                            );";

            using (SqlConnection connection = DbHelper.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                try
                {
                    connection.Open();

                    command.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        "Error SQL al desactivar clientes inactivos: "
                        + ex.Message);

                    throw;
                }
            }
        }
    }
}