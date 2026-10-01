using Microsoft.Data.SqlClient;

namespace ConsorApp.Datos
{
    public static class Conexion
    {
        private static readonly string cadenaConexion =
            @"Server=.\SQLEXPRESS;
              Database=consorAppDb;
              Trusted_Connection=True;
              TrustServerCertificate=True;";

        public static SqlConnection ObtenerConexion()
        {
            return new SqlConnection(cadenaConexion);
        }
    }
}