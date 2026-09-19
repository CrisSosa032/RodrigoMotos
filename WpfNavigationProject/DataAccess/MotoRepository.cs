using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
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
                    c.Nombre AS NombreCliente,
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
                ORDER BY 
                    m.Marca,
                    m.Modelo,
                    m.IdMoto";

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
                        "Error SQL al obtener motos: " + ex.Message);

                    throw;
                }
            }

            return motos;
        }


        // ============================================================
        // OBTENER MOTOS FILTRADAS Y PAGINADAS
        // ============================================================

        public List<Moto> GetMotosFiltradasPaginadas(
            string texto,
            string campoBusqueda,
            DateTime? fechaDesde,
            DateTime? fechaHasta,
            int pagina,
            int motosPorPagina)
        {
            List<Moto> motos = new List<Moto>();

            if (pagina < 1)
                pagina = 1;

            if (motosPorPagina < 1)
                motosPorPagina = 20;

            int offset = (pagina - 1) * motosPorPagina;

            string sql = @"
        SELECT
            m.IdMoto,
            m.IdCliente,
            c.Nombre AS NombreCliente,
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
            m.IdMoto

        OFFSET @Offset ROWS
        FETCH NEXT @MotosPorPagina ROWS ONLY;";

            using (SqlConnection connection =
                   DbHelper.CreateConnection())

            using (SqlCommand command =
                   new SqlCommand(sql, connection))
            {
                command.Parameters.Add(
                    "@Texto",
                    SqlDbType.NVarChar)
                    .Value = texto ?? string.Empty;

                command.Parameters.Add(
                    "@Campo",
                    SqlDbType.NVarChar)
                    .Value = campoBusqueda ?? "Marca";

                command.Parameters.Add(
                    "@FechaDesde",
                    SqlDbType.Date)
                    .Value = fechaDesde.HasValue
                        ? fechaDesde.Value.Date
                        : DBNull.Value;

                command.Parameters.Add(
                    "@FechaHasta",
                    SqlDbType.Date)
                    .Value = fechaHasta.HasValue
                        ? fechaHasta.Value.Date
                        : DBNull.Value;

                command.Parameters.Add(
                    "@Offset",
                    SqlDbType.Int)
                    .Value = offset;

                command.Parameters.Add(
                    "@MotosPorPagina",
                    SqlDbType.Int)
                    .Value = motosPorPagina;

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
                        "Error SQL al obtener motos paginadas: "
                        + ex.Message);

                    throw;
                }
            }

            return motos;
        }


        // ============================================================
        // CONTAR MOTOS FILTRADAS
        // ============================================================

        public int GetTotalMotosFiltradas(
            string texto,
            string campoBusqueda,
            DateTime? fechaDesde,
            DateTime? fechaHasta)
        {
            string sql = @"
        SELECT COUNT(*)
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
            );";

            using (SqlConnection connection =
                   DbHelper.CreateConnection())

            using (SqlCommand command =
                   new SqlCommand(sql, connection))
            {
                command.Parameters.Add(
                    "@Texto",
                    SqlDbType.NVarChar)
                    .Value = texto ?? string.Empty;

                command.Parameters.Add(
                    "@Campo",
                    SqlDbType.NVarChar)
                    .Value = campoBusqueda ?? "Marca";

                command.Parameters.Add(
                    "@FechaDesde",
                    SqlDbType.Date)
                    .Value = fechaDesde.HasValue
                        ? fechaDesde.Value.Date
                        : DBNull.Value;

                command.Parameters.Add(
                    "@FechaHasta",
                    SqlDbType.Date)
                    .Value = fechaHasta.HasValue
                        ? fechaHasta.Value.Date
                        : DBNull.Value;

                try
                {
                    connection.Open();

                    object resultado =
                        command.ExecuteScalar();

                    return resultado != null
                        ? Convert.ToInt32(resultado)
                        : 0;
                }
                catch (SqlException ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        "Error SQL al contar motos filtradas: "
                        + ex.Message);

                    throw;
                }
            }
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
                    c.Nombre AS NombreCliente,
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
                WHERE m.IdMoto = @IdMoto";

            using (SqlConnection connection =
                   DbHelper.CreateConnection())

            using (SqlCommand command =
                   new SqlCommand(sql, connection))
            {
                command.Parameters.Add(
                    "@IdMoto",
                    SqlDbType.Int)
                    .Value = idMoto;

                try
                {
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
                catch (SqlException ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        "Error SQL al obtener moto: " + ex.Message);

                    throw;
                }
            }

            return null;
        }


        // ============================================================
        // INSERTAR MOTO
        // ============================================================

        public int AddMoto(Moto moto)
        {
            using (SqlConnection connection =
                   DbHelper.CreateConnection())
            {
                try
                {
                    connection.Open();

                    // ------------------------------------------------
                    // VALIDAR CLIENTE
                    // ------------------------------------------------

                    string sqlValidarCliente = @"
                        SELECT Activo
                        FROM Clientes
                        WHERE IdCliente = @IdCliente";

                    using (SqlCommand commandValidar =
                           new SqlCommand(
                               sqlValidarCliente,
                               connection))
                    {
                        commandValidar.Parameters.Add(
                            "@IdCliente",
                            SqlDbType.Int)
                            .Value = moto.IdCliente;

                        object resultado =
                            commandValidar.ExecuteScalar();

                        if (resultado == null)
                        {
                            throw new Exception(
                                "No se encontró el cliente seleccionado.");
                        }

                        bool clienteActivo =
                            Convert.ToBoolean(resultado);

                        if (!clienteActivo)
                        {
                            throw new Exception(
                                "No se puede registrar la moto porque " +
                                "el cliente se encuentra dado de baja.");
                        }
                    }

                    // ------------------------------------------------
                    // INSERTAR MOTO
                    // ------------------------------------------------

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

                    using (SqlCommand command =
                           new SqlCommand(sql, connection))
                    {
                        command.Parameters.Add(
                            "@IdCliente",
                            SqlDbType.Int)
                            .Value = moto.IdCliente;

                        command.Parameters.Add(
                            "@Marca",
                            SqlDbType.NVarChar)
                            .Value = (object)moto.Marca
                                     ?? DBNull.Value;

                        command.Parameters.Add(
                            "@Modelo",
                            SqlDbType.NVarChar)
                            .Value = (object)moto.Modelo
                                     ?? DBNull.Value;

                        command.Parameters.Add(
                            "@Anio",
                            SqlDbType.SmallInt)
                            .Value = moto.Anio.HasValue
                                ? moto.Anio.Value
                                : DBNull.Value;

                        command.Parameters.Add(
                            "@Patente",
                            SqlDbType.NVarChar)
                            .Value = (object)moto.Patente
                                     ?? DBNull.Value;

                        command.Parameters.Add(
                            "@NroMotor",
                            SqlDbType.NVarChar)
                            .Value = (object)moto.NroMotor
                                     ?? DBNull.Value;

                        command.Parameters.Add(
                            "@NroChasis",
                            SqlDbType.NVarChar)
                            .Value = (object)moto.NroChasis
                                     ?? DBNull.Value;

                        command.Parameters.Add(
                            "@Observaciones",
                            SqlDbType.NVarChar)
                            .Value = (object)moto.Observaciones
                                     ?? DBNull.Value;

                        object result =
                            command.ExecuteScalar();

                        return result != null
                            ? Convert.ToInt32(result)
                            : 0;
                    }
                }
                catch (SqlException ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        "Error SQL al insertar moto: "
                        + ex.Message);

                    throw;
                }
            }
        }


        // ============================================================
        // ACTUALIZAR MOTO
        // ============================================================

        public void UpdateMoto(Moto moto)
        {
            using (SqlConnection connection =
                   DbHelper.CreateConnection())
            {
                try
                {
                    connection.Open();

                    // ------------------------------------------------
                    // VALIDAR CLIENTE
                    // ------------------------------------------------

                    string sqlValidarCliente = @"
                        SELECT Activo
                        FROM Clientes
                        WHERE IdCliente = @IdCliente";

                    using (SqlCommand commandValidar =
                           new SqlCommand(
                               sqlValidarCliente,
                               connection))
                    {
                        commandValidar.Parameters.Add(
                            "@IdCliente",
                            SqlDbType.Int)
                            .Value = moto.IdCliente;

                        object resultado =
                            commandValidar.ExecuteScalar();

                        if (resultado == null)
                        {
                            throw new Exception(
                                "No se encontró el cliente seleccionado.");
                        }

                        bool clienteActivo =
                            Convert.ToBoolean(resultado);

                        if (!clienteActivo)
                        {
                            throw new Exception(
                                "No se puede actualizar la moto porque " +
                                "el cliente se encuentra dado de baja.");
                        }
                    }

                    // ------------------------------------------------
                    // ACTUALIZAR MOTO
                    // ------------------------------------------------

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
                        WHERE IdMoto = @IdMoto";

                    using (SqlCommand command =
                           new SqlCommand(sql, connection))
                    {
                        command.Parameters.Add(
                            "@IdMoto",
                            SqlDbType.Int)
                            .Value = moto.IdMoto;

                        command.Parameters.Add(
                            "@IdCliente",
                            SqlDbType.Int)
                            .Value = moto.IdCliente;

                        command.Parameters.Add(
                            "@Marca",
                            SqlDbType.NVarChar)
                            .Value = (object)moto.Marca
                                     ?? DBNull.Value;

                        command.Parameters.Add(
                            "@Modelo",
                            SqlDbType.NVarChar)
                            .Value = (object)moto.Modelo
                                     ?? DBNull.Value;

                        command.Parameters.Add(
                            "@Anio",
                            SqlDbType.SmallInt)
                            .Value = moto.Anio.HasValue
                                ? moto.Anio.Value
                                : DBNull.Value;

                        command.Parameters.Add(
                            "@Patente",
                            SqlDbType.NVarChar)
                            .Value = (object)moto.Patente
                                     ?? DBNull.Value;

                        command.Parameters.Add(
                            "@NroMotor",
                            SqlDbType.NVarChar)
                            .Value = (object)moto.NroMotor
                                     ?? DBNull.Value;

                        command.Parameters.Add(
                            "@NroChasis",
                            SqlDbType.NVarChar)
                            .Value = (object)moto.NroChasis
                                     ?? DBNull.Value;

                        command.Parameters.Add(
                            "@Observaciones",
                            SqlDbType.NVarChar)
                            .Value = (object)moto.Observaciones
                                     ?? DBNull.Value;

                        command.ExecuteNonQuery();
                    }
                }
                catch (SqlException ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        "Error SQL al actualizar moto: "
                        + ex.Message);

                    throw;
                }
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
                command.Parameters.Add(
                    "@IdMoto",
                    SqlDbType.Int)
                    .Value = idMoto;

                try
                {
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
                catch (SqlException ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        "Error SQL al verificar cliente de la moto: "
                        + ex.Message);

                    throw;
                }
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
                command.Parameters.Add(
                    "@IdMoto",
                    SqlDbType.Int)
                    .Value = idMoto;

                try
                {
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
                catch (SqlException ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        "Error SQL al obtener el cliente de la moto: "
                        + ex.Message);

                    throw;
                }
            }
        }


        // ============================================================
        // MAPEAR MOTO
        // ============================================================

        private Moto MapearMoto(SqlDataReader reader)
        {
            return new Moto
            {
                IdMoto = reader.GetInt32(
                    reader.GetOrdinal("IdMoto")),

                IdCliente = reader.GetInt32(
                    reader.GetOrdinal("IdCliente")),

                NombreCliente = reader["NombreCliente"] != DBNull.Value
                    ? reader["NombreCliente"].ToString()!
                    : string.Empty,

                Marca = reader["Marca"] != DBNull.Value
                    ? reader["Marca"].ToString()!
                    : string.Empty,

                Modelo = reader["Modelo"] != DBNull.Value
                    ? reader["Modelo"].ToString()!
                    : string.Empty,

                Anio = reader["Anio"] != DBNull.Value
                    ? Convert.ToInt16(reader["Anio"])
                    : null,

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



        // ============================================================
        // OBTENER MOTOS DE UN CLIENTE
        // ============================================================

        public List<Moto> GetMotosByCliente(int idCliente)
        {
            List<Moto> motos = new List<Moto>();

            string sql = @"
                        SELECT
                            m.IdMoto,
                            m.IdCliente,
                            c.Nombre AS NombreCliente,
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
                        WHERE m.IdCliente = @IdCliente
                        ORDER BY
                            m.Marca,
                            m.Modelo,
                            m.IdMoto";

            using (SqlConnection connection =
                   DbHelper.CreateConnection())

            using (SqlCommand command =
                   new SqlCommand(sql, connection))
            {
                command.Parameters.Add(
                    "@IdCliente",
                    SqlDbType.Int)
                    .Value = idCliente;

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
                        "Error SQL al obtener motos del cliente: "
                        + ex.Message);

                    throw;
                }
            }

            return motos;
        }
    }
}