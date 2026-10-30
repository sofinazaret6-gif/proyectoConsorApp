using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;

namespace ConsorApp.Datos
{
    /// <summary>
    /// Sube respaldos a Google Drive del administrador.
    /// Requiere el archivo de credenciales OAuth (tipo "Aplicación de escritorio") en
    /// %AppData%\ConsorApp\google_credentials.json (ver README).
    /// Usa el alcance drive.file: la app solo ve lo que ella misma crea (carpeta "ConsorAppBackups").
    /// </summary>
    public class GoogleDriveRepositorio
    {
        private const string NombreCarpetaDrive = "ConsorAppBackups";
        private const string NombreArchivoCredenciales = "google_credentials.json";

        private static readonly string CarpetaToken = Path.Combine(
            ConfiguracionRespaldoRepositorio.CarpetaDatos, "drive-token");

        public bool EstaConectado =>
            Directory.Exists(CarpetaToken) && Directory.EnumerateFileSystemEntries(CarpetaToken).Any();

        /// <summary>Abre el navegador para que el administrador autorice el acceso.</summary>
        public async Task ConectarAsync()
        {
            await ObtenerCredencialAsync(CancellationToken.None);
        }

        public void Desconectar()
        {
            if (Directory.Exists(CarpetaToken))
                Directory.Delete(CarpetaToken, true);
        }

        /// <summary>Sube el archivo a la carpeta ConsorAppBackups y devuelve el nombre subido.</summary>
        public async Task<string> SubirAsync(string rutaArchivo)
        {
            if (!File.Exists(rutaArchivo))
                throw new FileNotFoundException("No se encontró el archivo a subir.", rutaArchivo);

            var credencial = await ObtenerCredencialAsync(CancellationToken.None);

            using var servicio = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credencial,
                ApplicationName = "ConsorApp"
            });

            string carpetaId = await ObtenerOCrearCarpetaAsync(servicio);

            var metadata = new Google.Apis.Drive.v3.Data.File
            {
                Name = Path.GetFileName(rutaArchivo),
                Parents = new List<string> { carpetaId }
            };

            await using var stream = new FileStream(rutaArchivo, FileMode.Open, FileAccess.Read, FileShare.Read);
            var peticion = servicio.Files.Create(metadata, stream, "application/octet-stream");
            peticion.Fields = "id, name";

            var progreso = await peticion.UploadAsync();
            if (progreso.Status != UploadStatus.Completed)
            {
                throw new InvalidOperationException(
                    "No se pudo subir el respaldo a Google Drive: " + progreso.Exception?.Message,
                    progreso.Exception);
            }

            return peticion.ResponseBody?.Name ?? metadata.Name;
        }

        private static async Task<string> ObtenerOCrearCarpetaAsync(DriveService servicio)
        {
            var lista = servicio.Files.List();
            lista.Q = $"mimeType = 'application/vnd.google-apps.folder' and name = '{NombreCarpetaDrive}' and trashed = false";
            lista.Fields = "files(id, name)";

            var resultado = await lista.ExecuteAsync();
            var existente = resultado.Files?.FirstOrDefault();
            if (existente != null)
                return existente.Id;

            var carpeta = new Google.Apis.Drive.v3.Data.File
            {
                Name = NombreCarpetaDrive,
                MimeType = "application/vnd.google-apps.folder"
            };
            var creada = await servicio.Files.Create(carpeta).ExecuteAsync();
            return creada.Id;
        }

        private static async Task<UserCredential> ObtenerCredencialAsync(CancellationToken ct)
        {
            string rutaCredenciales = BuscarArchivoCredenciales();
            var secretos = GoogleClientSecrets.FromFile(rutaCredenciales).Secrets;

            return await GoogleWebAuthorizationBroker.AuthorizeAsync(
                secretos,
                new[] { DriveService.Scope.DriveFile },
                "consorapp-admin",
                ct,
                new FileDataStore(CarpetaToken, true));
        }

        private static string BuscarArchivoCredenciales()
        {
            string enAppData = Path.Combine(ConfiguracionRespaldoRepositorio.CarpetaDatos, NombreArchivoCredenciales);
            if (File.Exists(enAppData)) return enAppData;

            string junto = Path.Combine(AppContext.BaseDirectory, NombreArchivoCredenciales);
            if (File.Exists(junto)) return junto;

            throw new FileNotFoundException(
                "Falta el archivo de credenciales de Google. Copialo como:\n" + enAppData +
                "\n\n(Los pasos para obtenerlo están en el README.)");
        }
    }
}
