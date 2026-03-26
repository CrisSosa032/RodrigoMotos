using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using WpfNavigationProject.Models; 

namespace WpfNavigationProject.DataAccess
{
    // Usamos DbHelper (que es static) para crear las conexiones
    public class ClienteRepository
    {
        /// <summary>
        /// Obtiene todos los clientes de la base de datos.
        /// </summary>
        /// <returns>Una lista de objetos Cliente.</returns>
        public List<Cliente> GetAllClientes()
        {
            List<Cliente> clientes = new List<Cliente>();

            // La consulta SELECT para obtener todas las columnas
            string sql = "SELECT IdCliente, Nombre, DNI, Direccion, Telefono FROM Clientes";

            // Usamos 'using' para asegurar que la conexión se cierre y se liberen recursos
            using (SqlConnection connection = DbHelper.CreateConnection())
            {
                // Usamos 'using' para asegurar que el comando se destruya
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    try
                    {
                        connection.Open();

                        // SqlDataReader lee los resultados fila por fila
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Mapeamos cada fila del lector a un nuevo objeto Cliente
                                clientes.Add(new Cliente
                                {
                                    // La forma más segura de leer es por el nombre de la columna,
                                    // asegurándose de que el tipo de dato coincida.
                                    IdCliente = reader.GetInt32(reader.GetOrdinal("IdCliente")),
                                    Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                                    DNI = reader.GetString(reader.GetOrdinal("DNI")),
                                    Direccion = reader.GetString(reader.GetOrdinal("Direccion")),
                                    Telefono = reader.GetString(reader.GetOrdinal("Telefono"))
                                });
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        // En un proyecto real, esto se manejaría con logging o 
                        // lanzando una excepción personalizada de la capa de acceso a datos.
                        System.Diagnostics.Debug.WriteLine("Error SQL: " + ex.Message);
                        throw; // Relanzamos para que la capa de presentación pueda manejarlo
                    }
                }
            }

            return clientes;
        }




        /// <summary>
        /// Inserta un nuevo cliente en la base de datos.
        /// </summary>
        /// <param name="cliente">Objeto Cliente con los datos a guardar.</param>
        /// <returns>El IdCliente generado por la base de datos.</returns>
        public int AddCliente(Cliente cliente)
        {
            // Usamos una consulta parametrizada para prevenir ataques de inyección SQL
            string sql = @"
            INSERT INTO Clientes (Nombre, DNI, Direccion, Telefono) 
            VALUES (@Nombre, @DNI, @Direccion, @Telefono);
            SELECT SCOPE_IDENTITY();"; // Recupera el ID generado

            using (SqlConnection connection = DbHelper.CreateConnection())
            {
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    // 1. Agregar Parámetros
                    // Los nombres (@Nombre, @DNI, etc.) deben coincidir con los de la consulta SQL.
                    command.Parameters.AddWithValue("@Nombre", cliente.Nombre);
                    command.Parameters.AddWithValue("@DNI", cliente.DNI);
                    // Manejo de valores NULL o vacíos para campos opcionales
                    command.Parameters.AddWithValue("@Direccion", (object)cliente.Direccion ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Telefono", (object)cliente.Telefono ?? DBNull.Value);

                    try
                    {
                        connection.Open();

                        // 2. Ejecutar la Inserción
                        // ExecuteScalar ejecuta la consulta y devuelve el primer valor de la primera fila.
                        // En este caso, devuelve el resultado de SELECT SCOPE_IDENTITY() (el IdCliente).
                        object result = command.ExecuteScalar();

                        // Convertir el resultado a entero.
                        return result != null ? Convert.ToInt32(result) : 0;
                    }
                    catch (SqlException ex)
                    {
                        // Manejo de error de base de datos
                        System.Diagnostics.Debug.WriteLine($"Error SQL al insertar cliente: {ex.Message}");
                        throw;
                    }
                }
            }
        }

        public Cliente GetClienteById(int id)
        {
            string sql = "SELECT IdCliente, Nombre, DNI, Telefono, Direccion FROM Clientes WHERE IdCliente = @Id";
            using (SqlConnection connection = DbHelper.CreateConnection())
            {
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
                                IdCliente = Convert.ToInt32(reader["IdCliente"]),
                                Nombre = reader["Nombre"].ToString() ?? string.Empty,
                                DNI = reader["DNI"].ToString() ?? string.Empty,
                                Telefono = reader["Telefono"].ToString() ?? string.Empty,
                                Direccion = reader["Direccion"].ToString() ?? string.Empty
                            };
                        }
                    }
                }
            }
            return null;
        }


        public void UpdateCliente(Cliente cliente)
        {
            string sql = @"UPDATE Clientes 
                   SET Nombre = @Nombre, DNI = @DNI, Telefono = @Telefono, Direccion = @Direccion 
                   WHERE IdCliente = @Id";

            using (SqlConnection connection = DbHelper.CreateConnection())
            {
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Nombre", cliente.Nombre);
                    command.Parameters.AddWithValue("@DNI", cliente.DNI);
                    command.Parameters.AddWithValue("@Telefono", cliente.Telefono);
                    command.Parameters.AddWithValue("@Direccion", cliente.Direccion);
                    command.Parameters.AddWithValue("@Id", cliente.IdCliente);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }


        public void DeleteCliente(int id)
        {
            string sql = "DELETE FROM Clientes WHERE IdCliente = @Id";

            using (SqlConnection connection = DbHelper.CreateConnection())
            {
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    
                    command.Parameters.AddWithValue("@Id", id);

                    connection.Open();
                    command.ExecuteNonQuery(); // Ejecutamos la orden de eliminación
                }
            }
        }


    }
}