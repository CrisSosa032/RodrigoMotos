using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using WpfNavigationProject.Models;
using BCrypt.Net;

namespace WpfNavigationProject.DataAccess
{
    public class UsuarioRepository
    {
        // =========================================================
        // VALIDAR LOGIN
        // =========================================================
        public Usuario? ValidarUsuario(string username, string password)
        {
            string usuarioLimpio = username != null
                ? username.Trim()
                : string.Empty;

            string sql = @"
                SELECT IdUsuario, Username, Password, Nombre, Rol 
                FROM Usuarios 
                WHERE Username = @User";

            using (SqlConnection connection = DbHelper.CreateConnection())
            {
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add("@User", SqlDbType.NVarChar, 50)
                                     .Value = usuarioLimpio;

                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string hashGuardado = reader["Password"].ToString()!;

                            // Comparamos la contraseña ingresada contra el hash
                            bool esValido = BCrypt.Net.BCrypt.Verify(
                                password,
                                hashGuardado
                            );

                            if (esValido)
                            {
                                return new Usuario
                                {
                                    IdUsuario = Convert.ToInt32(reader["IdUsuario"]),
                                    Username = reader["Username"].ToString()!,
                                    Nombre = reader["Nombre"].ToString()!,
                                    Rol = reader["Rol"].ToString()!
                                };
                            }
                        }
                    }
                }
            }

            return null;
        }


        // =========================================================
        // REGISTRAR USUARIO
        // =========================================================
        public void RegistrarUsuario(
            string username,
            string password,
            string nombre,
            string rol)
        {
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            string sql = @"
                INSERT INTO Usuarios
                (
                    Username,
                    Password,
                    Nombre,
                    Rol
                )
                VALUES
                (
                    @User,
                    @Pass,
                    @Nom,
                    @Rol
                )";

            using (SqlConnection connection = DbHelper.CreateConnection())
            {
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add("@User", SqlDbType.NVarChar, 50)
                                     .Value = username.Trim();

                    command.Parameters.Add("@Pass", SqlDbType.NVarChar, 255)
                                     .Value = passwordHash;

                    command.Parameters.Add("@Nom", SqlDbType.NVarChar, 100)
                                     .Value = nombre.Trim();

                    command.Parameters.Add("@Rol", SqlDbType.NVarChar, 50)
                                     .Value = rol.Trim();

                    connection.Open();

                    command.ExecuteNonQuery();
                }
            }
        }


        // =========================================================
        // OBTENER USUARIO POR USERNAME
        // =========================================================
        public Usuario? ObtenerUsuarioPorUsername(string username)
        {
            string usuarioLimpio = username != null
                ? username.Trim()
                : string.Empty;

            string sql = @"
                SELECT IdUsuario, Username, Nombre, Rol
                FROM Usuarios
                WHERE Username = @User";

            using (SqlConnection connection = DbHelper.CreateConnection())
            {
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add("@User", SqlDbType.NVarChar, 50)
                                     .Value = usuarioLimpio;

                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Usuario
                            {
                                IdUsuario = Convert.ToInt32(reader["IdUsuario"]),
                                Username = reader["Username"].ToString()!,
                                Nombre = reader["Nombre"].ToString()!,
                                Rol = reader["Rol"].ToString()!
                            };
                        }
                    }
                }
            }

            return null;
        }


        // =========================================================
        // ACTUALIZAR USUARIO
        // =========================================================
        // Actualiza Username, Nombre y Rol.
        //
        // Si nuevaPassword está vacía, la contraseña actual
        // permanece sin modificaciones.
        //
        // Si nuevaPassword tiene contenido, se genera un nuevo
        // hash BCrypt y se actualiza también la contraseña.
        // =========================================================
        public bool ActualizarUsuario(
            int idUsuario,
            string username,
            string nombre,
            string rol,
            string nuevaPassword)
        {
            username = username.Trim();
            nombre = nombre.Trim();
            rol = rol.Trim();

            string sql;

            if (string.IsNullOrWhiteSpace(nuevaPassword))
            {
                sql = @"
                    UPDATE Usuarios
                    SET
                        Username = @User,
                        Nombre = @Nom,
                        Rol = @Rol
                    WHERE IdUsuario = @IdUsuario";
            }
            else
            {
                string passwordHash =
                    BCrypt.Net.BCrypt.HashPassword(nuevaPassword);

                sql = @"
                    UPDATE Usuarios
                    SET
                        Username = @User,
                        Password = @Pass,
                        Nombre = @Nom,
                        Rol = @Rol
                    WHERE IdUsuario = @IdUsuario";
            }

            using (SqlConnection connection = DbHelper.CreateConnection())
            {
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add("@User", SqlDbType.NVarChar, 50)
                                     .Value = username;

                    command.Parameters.Add("@Nom", SqlDbType.NVarChar, 100)
                                     .Value = nombre;

                    command.Parameters.Add("@Rol", SqlDbType.NVarChar, 50)
                                     .Value = rol;

                    command.Parameters.Add("@IdUsuario", SqlDbType.Int)
                                     .Value = idUsuario;

                    // Solo agregamos la contraseña cuando realmente
                    // se solicitó cambiarla.
                    if (!string.IsNullOrWhiteSpace(nuevaPassword))
                    {
                        string passwordHash =
                            BCrypt.Net.BCrypt.HashPassword(nuevaPassword);

                        command.Parameters.Add(
                            "@Pass",
                            SqlDbType.NVarChar,
                            255
                        ).Value = passwordHash;
                    }

                    connection.Open();

                    int filasAfectadas = command.ExecuteNonQuery();

                    return filasAfectadas > 0;
                }
            }
        }


        // =========================================================
        // ACTUALIZAR CONTRASEÑA
        // =========================================================
        public bool ActualizarPassword(
            int idUsuario,
            string nuevaPassword)
        {
            string passwordHash =
                BCrypt.Net.BCrypt.HashPassword(nuevaPassword);

            string sql = @"
                UPDATE Usuarios
                SET Password = @Pass
                WHERE IdUsuario = @IdUsuario";

            using (SqlConnection connection = DbHelper.CreateConnection())
            {
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add("@Pass", SqlDbType.NVarChar, 255)
                                     .Value = passwordHash;

                    command.Parameters.Add("@IdUsuario", SqlDbType.Int)
                                     .Value = idUsuario;

                    connection.Open();

                    int filasAfectadas = command.ExecuteNonQuery();

                    return filasAfectadas > 0;
                }
            }
        }


        // =========================================================
        // OBTENER TODOS LOS USUARIOS
        // =========================================================
        public List<Usuario> ObtenerTodosLosUsuarios()
        {
            List<Usuario> usuarios = new List<Usuario>();

            string sql = @"
                SELECT IdUsuario, Username, Nombre, Rol
                FROM Usuarios
                ORDER BY IdUsuario";

            using (SqlConnection connection = DbHelper.CreateConnection())
            {
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Usuario usuario = new Usuario
                            {
                                IdUsuario = Convert.ToInt32(reader["IdUsuario"]),
                                Username = reader["Username"].ToString()!,
                                Nombre = reader["Nombre"].ToString()!,
                                Rol = reader["Rol"].ToString()!
                            };

                            usuarios.Add(usuario);
                        }
                    }
                }
            }

            return usuarios;
        }



        // =========================================================
        // OBTENER USUARIO POR ID
        // =========================================================
        public Usuario? ObtenerUsuarioPorId(int idUsuario)
        {
            string sql = @"
        SELECT IdUsuario, Username, Nombre, Rol
        FROM Usuarios
        WHERE IdUsuario = @IdUsuario";

            using (SqlConnection connection = DbHelper.CreateConnection())
            {
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add("@IdUsuario", SqlDbType.Int)
                                     .Value = idUsuario;

                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Usuario
                            {
                                IdUsuario = Convert.ToInt32(reader["IdUsuario"]),
                                Username = reader["Username"].ToString()!,
                                Nombre = reader["Nombre"].ToString()!,
                                Rol = reader["Rol"].ToString()!
                            };
                        }
                    }
                }
            }

            return null;
        }
    }
}