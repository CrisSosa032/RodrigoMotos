using System.Configuration;
using Microsoft.Data.SqlClient;

namespace WpfNavigationProject.DataAccess 
{
    public static class DbHelper
    {
        // 1. Nombre de la conexión definido en App.config
        private const string ConnectionName = "MiConexionBD";

        /// <summary>
        /// Obtiene la cadena de conexión directamente desde el App.config.
        /// </summary>
        /// <returns>La cadena de conexión como string.</returns>
        private static string GetConnectionString()
        {
            // La clase ConfigurationManager busca en el archivo .exe.config
            // la sección 'connectionStrings'
            ConnectionStringSettings settings =
                ConfigurationManager.ConnectionStrings[ConnectionName];

            if (settings == null)
            {
                // Si el nombre es incorrecto o no existe en App.config
                throw new ConfigurationErrorsException($"No se encontró la cadena de conexión con el nombre '{ConnectionName}' en App.config.");
            }

            return settings.ConnectionString;
        }

        /// <summary>
        /// Crea y devuelve un objeto SqlConnection listo para ser utilizado.
        /// </summary>
        /// <returns>Una nueva instancia de SqlConnection.</returns>
        public static SqlConnection CreateConnection()
        {
            // Llama al método privado para obtener la cadena
            string connectionString = GetConnectionString();

            // Crea y devuelve la conexión
            return new SqlConnection(connectionString);
        }

    }
}