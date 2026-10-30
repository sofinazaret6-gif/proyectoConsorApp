using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ConsorApp.Datos
{
    /// <summary>
    /// Ejecuta BACKUP / RESTORE contra SQL Server.
    /// Las rutas que recibe son las que ve el SERVIDOR SQL (no necesariamente la PC de la app).
    /// </summary>
    public class RespaldoSqlRepositorio
    {
        private readonly string _cadenaMaster;

        public string NombreBaseDatos { get; }

        private const string CadenaConexion =
    @"Server=.\SQLEXPRESS;Database=consorAppDb;Integrated Security=True;TrustServerCertificate=True;";

        public RespaldoSqlRepositorio()
        {
            var builder = new SqlConnectionStringBuilder(CadenaConexion);

            NombreBaseDatos = builder.InitialCatalog;   // consorAppDb
            builder.InitialCatalog = "master";          // BACKUP/RESTORE se ejecutan desde master
            _cadenaMaster = builder.ConnectionString;
        }

        /// <summary>Carpeta de backups por defecto de la instancia (ya tiene permisos para SQL Server).</summary>
        public string ObtenerRutaBackupPorDefecto()
        {
            try
            {
                using var cn = new SqlConnection(_cadenaMaster);
                cn.Open();
                using var cmd = new SqlCommand(
                    "SELECT CAST(SERVERPROPERTY('InstanceDefaultBackupPath') AS nvarchar(512));", cn);
                object resultado = cmd.ExecuteScalar();
                return resultado == null || resultado == DBNull.Value ? "" : Convert.ToString(resultado) ?? "";
            }
            catch
            {
                return "";
            }
        }

        public async Task CrearBackupAsync(string rutaArchivoSql)
        {
            string db = Corchetes(NombreBaseDatos);

            try
            {
                await using var cn = new SqlConnection(_cadenaMaster);
                await cn.OpenAsync();

                await using var cmd = new SqlCommand(
                    $"BACKUP DATABASE {db} TO DISK = @ruta WITH INIT, CHECKSUM, NAME = @nombre;", cn);
                cmd.CommandTimeout = 900;
                cmd.Parameters.AddWithValue("@ruta", rutaArchivoSql);
                cmd.Parameters.AddWithValue("@nombre", $"{NombreBaseDatos} - Backup completo");
                await cmd.ExecuteNonQueryAsync();
            }
            catch (SqlException ex) when (ex.Number == 3201)
            {
                throw ErrorAccesoCarpeta(ex);
            }
        }

        /// <summary>RESTORE VERIFYONLY: comprueba que el .bak sea legible y esté íntegro (no toca la base).</summary>
        public async Task VerificarBackupAsync(string rutaArchivoSql)
        {
            try
            {
                await using var cn = new SqlConnection(_cadenaMaster);
                await cn.OpenAsync();

                await using var cmd = new SqlCommand(
                    "RESTORE VERIFYONLY FROM DISK = @ruta WITH CHECKSUM;", cn);
                cmd.CommandTimeout = 900;
                cmd.Parameters.AddWithValue("@ruta", rutaArchivoSql);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (SqlException ex) when (ex.Number == 3201)
            {
                throw ErrorAccesoCarpeta(ex);
            }
        }

        /// <summary>Reemplaza la base actual por el contenido del .bak. Expulsa las otras conexiones.</summary>
        public async Task RestaurarAsync(string rutaArchivoSql)
        {
            string db = Corchetes(NombreBaseDatos);

            await using var cn = new SqlConnection(_cadenaMaster);
            await cn.OpenAsync();

            try
            {
                await EjecutarAsync(cn, $"ALTER DATABASE {db} SET SINGLE_USER WITH ROLLBACK IMMEDIATE;");

                await using var cmd = new SqlCommand(
                    $"RESTORE DATABASE {db} FROM DISK = @ruta WITH REPLACE;", cn);
                cmd.CommandTimeout = 900;
                cmd.Parameters.AddWithValue("@ruta", rutaArchivoSql);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (SqlException ex) when (ex.Number == 3201)
            {
                throw ErrorAccesoCarpeta(ex);
            }
            finally
            {
                // Siempre dejar la base en modo normal, incluso si falló el restore
                try { await EjecutarAsync(cn, $"ALTER DATABASE {db} SET MULTI_USER;"); }
                catch { /* si la base no existe o no se pudo, no tapamos el error original */ }

                SqlConnection.ClearAllPools();
            }
        }

        private static async Task EjecutarAsync(SqlConnection cn, string sql)
        {
            await using var cmd = new SqlCommand(sql, cn) { CommandTimeout = 120 };
            await cmd.ExecuteNonQueryAsync();
        }

        private static string Corchetes(string nombre) => "[" + nombre.Replace("]", "]]") + "]";

        private static Exception ErrorAccesoCarpeta(SqlException ex) =>
            new InvalidOperationException(
                "SQL Server no pudo acceder al archivo o la carpeta del respaldo. " +
                "Verificá que la carpeta exista y que la cuenta del servicio de SQL Server " +
                "tenga permiso de lectura y escritura sobre ella.\n\nDetalle: " + ex.Message, ex);
    }
}
