using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using WpfNavigationProject.Models;

namespace WpfNavigationProject.DataAccess
{
    public class ServiciosRepository
    {
        // ============================================================
        // OBTENER TODOS LOS SERVICIOS
        // ============================================================

        public List<Servicios> GetAllServicios()
        {
            List<Servicios> serviciosList =
                new List<Servicios>();

            string sql = @"
                SELECT
                    s.IdServicio,
                    s.IdMoto,
                    s.IdEstado,
                    s.Detalle,
                    s.FechaEntrada,
                    s.CostoEstimado,

                    c.Nombre AS NombreCliente,

                    m.Marca AS MarcaMoto,
                    m.Modelo AS ModeloMoto,

                    e.NombreEstado AS NombreEstado

                FROM Servicios s

                INNER JOIN Motos m
                    ON s.IdMoto = m.IdMoto

                INNER JOIN Clientes c
                    ON m.IdCliente = c.IdCliente

                INNER JOIN EstadosServicio e
                    ON s.IdEstado = e.IdEstado

                ORDER BY s.FechaEntrada DESC";

            using (SqlConnection connection =
                   DbHelper.CreateConnection())
            {
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
                                serviciosList.Add(new Servicios
                                {
                                    IdServicio =
                                        Convert.ToInt32(
                                            reader["IdServicio"]),

                                    IdMoto =
                                        Convert.ToInt32(
                                            reader["IdMoto"]),

                                    IdEstado =
                                        Convert.ToInt32(
                                            reader["IdEstado"]),

                                    Detalle =
                                        reader["Detalle"] != DBNull.Value
                                            ? reader["Detalle"].ToString()
                                                ?? string.Empty
                                            : string.Empty,

                                    FechaEntrada =
                                        Convert.ToDateTime(
                                            reader["FechaEntrada"]),

                                    CostoEstimado =
                                        reader["CostoEstimado"] != DBNull.Value
                                            ? Convert.ToDecimal(
                                                reader["CostoEstimado"])
                                            : (decimal?)null,

                                    NombreCliente =
                                        reader["NombreCliente"] != DBNull.Value
                                            ? reader["NombreCliente"].ToString()
                                                ?? string.Empty
                                            : string.Empty,

                                    MarcaMoto =
                                        reader["MarcaMoto"] != DBNull.Value
                                            ? reader["MarcaMoto"].ToString()
                                                ?? string.Empty
                                            : string.Empty,

                                    ModeloMoto =
                                        reader["ModeloMoto"] != DBNull.Value
                                            ? reader["ModeloMoto"].ToString()
                                                ?? string.Empty
                                            : string.Empty,

                                    NombreEstado =
                                        reader["NombreEstado"] != DBNull.Value
                                            ? reader["NombreEstado"].ToString()
                                                ?? string.Empty
                                            : string.Empty
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

            return serviciosList;
        }


        // ============================================================
        // OBTENER SERVICIOS FILTRADOS
        // ============================================================

        public List<Servicios> GetServiciosFiltrados(
            string texto,
            string buscarPor,
            int? idEstado,
            DateTime? fechaDesde,
            DateTime? fechaHasta)
        {
            List<Servicios> serviciosList =
                new List<Servicios>();

            string sql = @"
                SELECT
                    s.IdServicio,
                    s.IdMoto,
                    s.IdEstado,
                    s.Detalle,
                    s.FechaEntrada,
                    s.CostoEstimado,

                    c.Nombre AS NombreCliente,

                    m.Marca AS MarcaMoto,
                    m.Modelo AS ModeloMoto,

                    e.NombreEstado AS NombreEstado

                FROM Servicios s

                INNER JOIN Motos m
                    ON s.IdMoto = m.IdMoto

                INNER JOIN Clientes c
                    ON m.IdCliente = c.IdCliente

                INNER JOIN EstadosServicio e
                    ON s.IdEstado = e.IdEstado

                WHERE
                    (
                        @Texto = ''
                        OR
                        (
                            @BuscarPor = 'Nombre'
                            AND c.Nombre LIKE '%' + @Texto + '%'
                        )
                        OR
                        (
                            @BuscarPor = 'Marca'
                            AND m.Marca LIKE '%' + @Texto + '%'
                        )
                        OR
                        (
                            @BuscarPor = 'Modelo'
                            AND m.Modelo LIKE '%' + @Texto + '%'
                        )
                    )

                    AND
                    (
                        @IdEstado IS NULL
                        OR s.IdEstado = @IdEstado
                    )

                    AND
                    (
                        @FechaDesde IS NULL
                        OR s.FechaEntrada >= @FechaDesde
                    )

                    AND
                    (
                        @FechaHasta IS NULL
                        OR s.FechaEntrada <= @FechaHasta
                    )

                ORDER BY s.FechaEntrada DESC";

            using (SqlConnection connection =
                   DbHelper.CreateConnection())
            {
                using (SqlCommand command =
                       new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue(
                        "@Texto",
                        texto ?? string.Empty);

                    command.Parameters.AddWithValue(
                        "@BuscarPor",
                        buscarPor ?? "Nombre");

                    command.Parameters.Add(
                        new SqlParameter(
                            "@IdEstado",
                            SqlDbType.Int)
                        {
                            Value = idEstado.HasValue
                                ? (object)idEstado.Value
                                : DBNull.Value
                        });

                    command.Parameters.Add(
                        new SqlParameter(
                            "@FechaDesde",
                            SqlDbType.Date)
                        {
                            Value = fechaDesde.HasValue
                                ? (object)fechaDesde.Value.Date
                                : DBNull.Value
                        });

                    command.Parameters.Add(
                        new SqlParameter(
                            "@FechaHasta",
                            SqlDbType.Date)
                        {
                            Value = fechaHasta.HasValue
                                ? (object)fechaHasta.Value.Date
                                : DBNull.Value
                        });

                    connection.Open();

                    using (SqlDataReader reader =
                           command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            serviciosList.Add(new Servicios
                            {
                                IdServicio =
                                    Convert.ToInt32(
                                        reader["IdServicio"]),

                                IdMoto =
                                    Convert.ToInt32(
                                        reader["IdMoto"]),

                                IdEstado =
                                    Convert.ToInt32(
                                        reader["IdEstado"]),

                                Detalle =
                                    reader["Detalle"] != DBNull.Value
                                        ? reader["Detalle"].ToString()
                                            ?? string.Empty
                                        : string.Empty,

                                FechaEntrada =
                                    Convert.ToDateTime(
                                        reader["FechaEntrada"]),

                                CostoEstimado =
                                    reader["CostoEstimado"] != DBNull.Value
                                        ? Convert.ToDecimal(
                                            reader["CostoEstimado"])
                                        : (decimal?)null,

                                NombreCliente =
                                    reader["NombreCliente"] != DBNull.Value
                                        ? reader["NombreCliente"].ToString()
                                            ?? string.Empty
                                        : string.Empty,

                                MarcaMoto =
                                    reader["MarcaMoto"] != DBNull.Value
                                        ? reader["MarcaMoto"].ToString()
                                            ?? string.Empty
                                        : string.Empty,

                                ModeloMoto =
                                    reader["ModeloMoto"] != DBNull.Value
                                        ? reader["ModeloMoto"].ToString()
                                            ?? string.Empty
                                        : string.Empty,

                                NombreEstado =
                                    reader["NombreEstado"] != DBNull.Value
                                        ? reader["NombreEstado"].ToString()
                                            ?? string.Empty
                                        : string.Empty
                            });
                        }
                    }
                }
            }

            return serviciosList;
        }


        // ============================================================
        // INSERTAR SERVICIO
        // ============================================================

        public int AddServicio(Servicios servicio)
        {
            string sql = @"
                INSERT INTO Servicios
                (
                    IdMoto,
                    IdEstado,
                    Detalle,
                    CostoEstimado
                )
                VALUES
                (
                    @IdMoto,
                    @IdEstado,
                    @Detalle,
                    @CostoEstimado
                );

                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (SqlConnection connection =
                   DbHelper.CreateConnection())
            {
                connection.Open();

                using (SqlTransaction transaction =
                       connection.BeginTransaction())
                {
                    try
                    {
                        string dependenciaSql = @"
                            SELECT
                                m.IdMoto,
                                c.Activo AS ClienteActivo
                            FROM Motos m
                            INNER JOIN Clientes c
                                ON m.IdCliente = c.IdCliente
                            WHERE m.IdMoto = @IdMoto";

                        using (SqlCommand dependenciaCommand =
                            new SqlCommand(
                                dependenciaSql,
                                connection,
                                transaction))
                        {
                            dependenciaCommand.Parameters.AddWithValue(
                                "@IdMoto",
                                servicio.IdMoto);

                            using (SqlDataReader reader =
                                   dependenciaCommand.ExecuteReader())
                            {
                                if (!reader.Read())
                                {
                                    throw new Exception(
                                        "No se encontró la moto seleccionada.");
                                }

                                bool clienteActivo =
                                    Convert.ToBoolean(
                                        reader["ClienteActivo"]);

                                if (!clienteActivo)
                                {
                                    throw new Exception(
                                        "No se puede generar un nuevo servicio " +
                                        "porque el cliente se encuentra dado de baja.");
                                }
                            }
                        }

                        using (SqlCommand command =
                            new SqlCommand(
                                sql,
                                connection,
                                transaction))
                        {
                            command.Parameters.AddWithValue(
                                "@IdMoto",
                                servicio.IdMoto);

                            command.Parameters.AddWithValue(
                                "@IdEstado",
                                servicio.IdEstado);

                            command.Parameters.AddWithValue(
                                "@Detalle",
                                (object)servicio.Detalle ??
                                DBNull.Value);

                            if (servicio.CostoEstimado.HasValue)
                            {
                                command.Parameters.Add(
                                    new SqlParameter(
                                        "@CostoEstimado",
                                        SqlDbType.Decimal)
                                    {
                                        Precision = 10,
                                        Scale = 2,
                                        Value =
                                            servicio.CostoEstimado.Value
                                    });
                            }
                            else
                            {
                                command.Parameters.AddWithValue(
                                    "@CostoEstimado",
                                    DBNull.Value);
                            }

                            int idServicio =
                                Convert.ToInt32(
                                    command.ExecuteScalar());

                            string historialSql = @"
                                INSERT INTO ServicioHistorial
                                (
                                    IdServicio,
                                    IdEstado
                                )
                                VALUES
                                (
                                    @IdServicio,
                                    @IdEstado
                                );";

                            using (SqlCommand historialCommand =
                                new SqlCommand(
                                    historialSql,
                                    connection,
                                    transaction))
                            {
                                historialCommand.Parameters.AddWithValue(
                                    "@IdServicio",
                                    idServicio);

                                historialCommand.Parameters.AddWithValue(
                                    "@IdEstado",
                                    servicio.IdEstado);

                                historialCommand.ExecuteNonQuery();
                            }

                            transaction.Commit();

                            return idServicio;
                        }
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback();

                        System.Diagnostics.Debug.WriteLine(
                            $"Error SQL al insertar servicio: {ex.Message}");

                        throw;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }


        // ============================================================
        // OBTENER SERVICIO POR ID
        // ============================================================

        public Servicios? GetServicioById(int id)
        {
            string sql = @"
                SELECT
                    s.IdServicio,
                    s.IdMoto,
                    s.IdEstado,
                    s.Detalle,
                    s.FechaEntrada,
                    s.CostoEstimado,

                    c.Nombre AS NombreCliente,

                    m.Marca AS MarcaMoto,
                    m.Modelo AS ModeloMoto,

                    e.NombreEstado AS NombreEstado

                FROM Servicios s

                INNER JOIN Motos m
                    ON s.IdMoto = m.IdMoto

                INNER JOIN Clientes c
                    ON m.IdCliente = c.IdCliente

                INNER JOIN EstadosServicio e
                    ON s.IdEstado = e.IdEstado

                WHERE s.IdServicio = @Id";

            using (SqlConnection connection =
                   DbHelper.CreateConnection())
            {
                using (SqlCommand command =
                       new SqlCommand(
                           sql,
                           connection))
                {
                    command.Parameters.AddWithValue(
                        "@Id",
                        id);

                    connection.Open();

                    using (SqlDataReader reader =
                           command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Servicios
                            {
                                IdServicio =
                                    Convert.ToInt32(
                                        reader["IdServicio"]),

                                IdMoto =
                                    Convert.ToInt32(
                                        reader["IdMoto"]),

                                IdEstado =
                                    Convert.ToInt32(
                                        reader["IdEstado"]),

                                Detalle =
                                    reader["Detalle"] != DBNull.Value
                                        ? reader["Detalle"].ToString()
                                            ?? string.Empty
                                        : string.Empty,

                                FechaEntrada =
                                    Convert.ToDateTime(
                                        reader["FechaEntrada"]),

                                CostoEstimado =
                                    reader["CostoEstimado"] != DBNull.Value
                                        ? Convert.ToDecimal(
                                            reader["CostoEstimado"])
                                        : (decimal?)null,

                                NombreCliente =
                                    reader["NombreCliente"] != DBNull.Value
                                        ? reader["NombreCliente"].ToString()
                                            ?? string.Empty
                                        : string.Empty,

                                MarcaMoto =
                                    reader["MarcaMoto"] != DBNull.Value
                                        ? reader["MarcaMoto"].ToString()
                                            ?? string.Empty
                                        : string.Empty,

                                ModeloMoto =
                                    reader["ModeloMoto"] != DBNull.Value
                                        ? reader["ModeloMoto"].ToString()
                                            ?? string.Empty
                                        : string.Empty,

                                NombreEstado =
                                    reader["NombreEstado"] != DBNull.Value
                                        ? reader["NombreEstado"].ToString()
                                            ?? string.Empty
                                        : string.Empty
                            };
                        }
                    }
                }
            }

            return null;
        }


        // ============================================================
        // ACTUALIZAR SERVICIO
        // ============================================================

        public void UpdateServicio(Servicios servicio)
        {
            using (SqlConnection connection =
                   DbHelper.CreateConnection())
            {
                connection.Open();

                using (SqlTransaction transaction =
                       connection.BeginTransaction())
                {
                    try
                    {
                        string estadoActualSql = @"
                            SELECT
                                IdEstado,
                                CostoEstimado
                            FROM Servicios WITH (UPDLOCK)
                            WHERE IdServicio = @IdServicio";

                        int estadoAnterior;
                        decimal? costoActual;

                        using (SqlCommand estadoCommand =
                            new SqlCommand(
                                estadoActualSql,
                                connection,
                                transaction))
                        {
                            estadoCommand.Parameters.AddWithValue(
                                "@IdServicio",
                                servicio.IdServicio);

                            using (SqlDataReader reader =
                                   estadoCommand.ExecuteReader())
                            {
                                if (!reader.Read())
                                {
                                    throw new Exception(
                                        "No se encontró el servicio.");
                                }

                                estadoAnterior =
                                    Convert.ToInt32(
                                        reader["IdEstado"]);

                                costoActual =
                                    reader["CostoEstimado"] != DBNull.Value
                                        ? Convert.ToDecimal(
                                            reader["CostoEstimado"])
                                        : (decimal?)null;
                            }
                        }

                        string dependenciaSql = @"
                            SELECT
                                m.IdMoto,
                                c.Activo AS ClienteActivo
                            FROM Motos m
                            INNER JOIN Clientes c
                                ON m.IdCliente = c.IdCliente
                            WHERE m.IdMoto = @IdMoto";

                        using (SqlCommand dependenciaCommand =
                            new SqlCommand(
                                dependenciaSql,
                                connection,
                                transaction))
                        {
                            dependenciaCommand.Parameters.AddWithValue(
                                "@IdMoto",
                                servicio.IdMoto);

                            using (SqlDataReader reader =
                                   dependenciaCommand.ExecuteReader())
                            {
                                if (!reader.Read())
                                {
                                    throw new Exception(
                                        "No se encontró la moto seleccionada.");
                                }

                                bool clienteActivo =
                                    Convert.ToBoolean(
                                        reader["ClienteActivo"]);

                                if (!clienteActivo)
                                {
                                    throw new Exception(
                                        "No se puede asociar el servicio " +
                                        "a una moto cuyo cliente está dado de baja.");
                                }
                            }
                        }

                        string updateSql = @"
                            UPDATE Servicios
                            SET
                                IdMoto = @IdMoto,
                                IdEstado = @IdEstado,
                                Detalle = @Detalle,
                                CostoEstimado = @CostoEstimado
                            WHERE IdServicio = @IdServicio";

                        using (SqlCommand updateCommand =
                            new SqlCommand(
                                updateSql,
                                connection,
                                transaction))
                        {
                            updateCommand.Parameters.AddWithValue(
                                "@IdMoto",
                                servicio.IdMoto);

                            updateCommand.Parameters.AddWithValue(
                                "@IdEstado",
                                servicio.IdEstado);

                            updateCommand.Parameters.AddWithValue(
                                "@Detalle",
                                (object)servicio.Detalle ??
                                DBNull.Value);

                            if (servicio.CostoEstimado.HasValue)
                            {
                                updateCommand.Parameters.Add(
                                    new SqlParameter(
                                        "@CostoEstimado",
                                        SqlDbType.Decimal)
                                    {
                                        Precision = 10,
                                        Scale = 2,
                                        Value =
                                            servicio.CostoEstimado.Value
                                    });
                            }
                            else
                            {
                                updateCommand.Parameters.AddWithValue(
                                    "@CostoEstimado",
                                    DBNull.Value);
                            }

                            updateCommand.Parameters.AddWithValue(
                                "@IdServicio",
                                servicio.IdServicio);

                            updateCommand.ExecuteNonQuery();
                        }

                        if (estadoAnterior != servicio.IdEstado)
                        {
                            string historialSql = @"
                                INSERT INTO ServicioHistorial
                                (
                                    IdServicio,
                                    IdEstado
                                )
                                VALUES
                                (
                                    @IdServicio,
                                    @IdEstado
                                );";

                            using (SqlCommand historialCommand =
                                new SqlCommand(
                                    historialSql,
                                    connection,
                                    transaction))
                            {
                                historialCommand.Parameters.AddWithValue(
                                    "@IdServicio",
                                    servicio.IdServicio);

                                historialCommand.Parameters.AddWithValue(
                                    "@IdEstado",
                                    servicio.IdEstado);

                                historialCommand.ExecuteNonQuery();
                            }
                        }

                        const int ESTADO_PAGADO = 4;

                        if (estadoAnterior != ESTADO_PAGADO &&
                            servicio.IdEstado == ESTADO_PAGADO)
                        {
                            decimal? montoGanancia =
                                servicio.CostoEstimado ??
                                costoActual;

                            if (!montoGanancia.HasValue)
                            {
                                throw new Exception(
                                    "No se puede marcar el servicio como Pagado " +
                                    "porque no tiene un costo registrado.");
                            }

                            string gananciaSql = @"
                                INSERT INTO Ganancias
                                (
                                    IdServicio,
                                    Monto
                                )
                                VALUES
                                (
                                    @IdServicio,
                                    @Monto
                                );";

                            using (SqlCommand gananciaCommand =
                                new SqlCommand(
                                    gananciaSql,
                                    connection,
                                    transaction))
                            {
                                gananciaCommand.Parameters.AddWithValue(
                                    "@IdServicio",
                                    servicio.IdServicio);

                                gananciaCommand.Parameters.Add(
                                    new SqlParameter(
                                        "@Monto",
                                        SqlDbType.Decimal)
                                    {
                                        Precision = 10,
                                        Scale = 2,
                                        Value =
                                            montoGanancia.Value
                                    });

                                gananciaCommand.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }


        // ============================================================
        // OBTENER ID MOTO DE UN SERVICIO
        // ============================================================

        public int GetIdMotoByServicio(int idServicio)
        {
            string sql = @"
                SELECT IdMoto
                FROM Servicios
                WHERE IdServicio = @IdServicio";

            using (SqlConnection connection =
                   DbHelper.CreateConnection())

            using (SqlCommand command =
                   new SqlCommand(
                       sql,
                       connection))
            {
                command.Parameters.AddWithValue(
                    "@IdServicio",
                    idServicio);

                connection.Open();

                object resultado =
                    command.ExecuteScalar();

                if (resultado == null)
                {
                    throw new Exception(
                        "No se encontró el servicio.");
                }

                return Convert.ToInt32(resultado);
            }
        }


        // ============================================================
        // OBTENER TRABAJOS EN CURSO
        // ============================================================

        public List<TrabajoEnCurso> GetTrabajosEnCurso()
        {
            List<TrabajoEnCurso> trabajos =
                new List<TrabajoEnCurso>();

            string sql = @"
                SELECT
                    s.IdServicio,
                    s.IdMoto,
                    s.IdEstado,
                    s.Detalle,
                    s.FechaEntrada,
                    m.Marca,
                    m.Modelo,
                    m.Patente,
                    c.Nombre AS Cliente
                FROM Servicios s
                INNER JOIN Motos m
                    ON s.IdMoto = m.IdMoto
                INNER JOIN Clientes c
                    ON m.IdCliente = c.IdCliente
                WHERE s.IdEstado = 2
                  AND c.Activo = 1
                ORDER BY s.FechaEntrada ASC";

            using (SqlConnection connection =
                   DbHelper.CreateConnection())

            using (SqlCommand command =
                   new SqlCommand(
                       sql,
                       connection))
            {
                connection.Open();

                using (SqlDataReader reader =
                       command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        trabajos.Add(new TrabajoEnCurso
                        {
                            IdServicio =
                                Convert.ToInt32(
                                    reader["IdServicio"]),

                            IdMoto =
                                Convert.ToInt32(
                                    reader["IdMoto"]),

                            IdEstado =
                                Convert.ToInt32(
                                    reader["IdEstado"]),

                            Detalle =
                                reader["Detalle"] != DBNull.Value
                                    ? reader["Detalle"].ToString()
                                        ?? string.Empty
                                    : string.Empty,

                            FechaEntrada =
                                Convert.ToDateTime(
                                    reader["FechaEntrada"]),

                            Marca =
                                reader["Marca"] != DBNull.Value
                                    ? reader["Marca"].ToString()
                                        ?? string.Empty
                                    : string.Empty,

                            Modelo =
                                reader["Modelo"] != DBNull.Value
                                    ? reader["Modelo"].ToString()
                                        ?? string.Empty
                                    : string.Empty,

                            Patente =
                                reader["Patente"] != DBNull.Value
                                    ? reader["Patente"].ToString()
                                        ?? string.Empty
                                    : string.Empty,

                            Cliente =
                                reader["Cliente"] != DBNull.Value
                                    ? reader["Cliente"].ToString()
                                        ?? string.Empty
                                    : string.Empty
                        });
                    }
                }
            }

            return trabajos;
        }
    }
}