using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ConsorApp.Datos;
using ConsorApp.Entidades;

namespace ConsorApp.Negocio
{
    /// <summary>
    /// Lógica de respaldos: crear, verificar, listar, subir a Drive y restaurar.
    /// Las ventanas solo hablan con esta clase.
    /// </summary>
    public class RespaldoService
    {
        private readonly ConfiguracionRespaldoRepositorio _config = new ConfiguracionRespaldoRepositorio();
        private readonly RespaldoSqlRepositorio _sql = new RespaldoSqlRepositorio();
        private readonly GoogleDriveRepositorio _drive = new GoogleDriveRepositorio();

        // ---------- Configuración ----------

        public ConfiguracionRespaldo ObtenerConfiguracion()
        {
            var cfg = _config.Cargar();
            cfg.BaseDatos = _sql.NombreBaseDatos;

            // Primera vez: proponer la carpeta de backups por defecto de la instancia (ya tiene permisos)
            if (string.IsNullOrWhiteSpace(cfg.RutaServidor))
                cfg.RutaServidor = _sql.ObtenerRutaBackupPorDefecto();

            return cfg;
        }

        public void GuardarRutas(string rutaServidor, string rutaCompartida)
        {
            var cfg = _config.Cargar();
            cfg.RutaServidor = (rutaServidor ?? "").Trim();
            cfg.RutaCompartida = (rutaCompartida ?? "").Trim();
            _config.Guardar(cfg);
        }

        // ---------- Google Drive ----------

        public bool DriveConectado => _drive.EstaConectado;

        public Task ConectarDriveAsync() => _drive.ConectarAsync();

        public void DesconectarDrive() => _drive.Desconectar();

        // ---------- Respaldos ----------

        public List<ArchivoRespaldo> ListarRespaldos()
        {
            var cfg = ObtenerConfiguracion();
            ValidarRutas(cfg);

            string carpeta = CarpetaVisible(cfg);
            if (!Directory.Exists(carpeta))
            {
                throw new DirectoryNotFoundException(
                    $"La carpeta '{carpeta}' no existe o la aplicación no puede acceder a ella. " +
                    "Revisá la configuración de respaldos.");
            }

            return new DirectoryInfo(carpeta)
                .GetFiles($"{cfg.BaseDatos}_*.bak")
                .OrderByDescending(f => f.LastWriteTime)
                .Select(f => new ArchivoRespaldo
                {
                    Nombre = f.Name,
                    Fecha = f.LastWriteTime,
                    TamanoBytes = f.Length
                })
                .ToList();
        }

        /// <summary>Crea un backup completo, lo verifica y devuelve el nombre del archivo.</summary>
        public async Task<string> HacerBackupAsync()
        {
            var cfg = ObtenerConfiguracion();
            ValidarRutas(cfg);

            string nombre = $"{cfg.BaseDatos}_{DateTime.Now:yyyyMMdd_HHmmss}_FULL.bak";
            string rutaSql = Path.Combine(cfg.RutaServidor.Trim(), nombre);

            try { Directory.CreateDirectory(CarpetaVisible(cfg)); }
            catch { /* si el servidor es remoto, la carpeta debe existir ya */ }

            await _sql.CrearBackupAsync(rutaSql);
            await _sql.VerificarBackupAsync(rutaSql);
            return nombre;
        }

        public async Task ProbarBackupAsync(ArchivoRespaldo archivo)
        {
            var cfg = ObtenerConfiguracion();
            ValidarRutas(cfg);
            await _sql.VerificarBackupAsync(Path.Combine(cfg.RutaServidor.Trim(), archivo.Nombre));
        }

        /// <summary>Sube el archivo a Google Drive y guarda cuándo se hizo.</summary>
        public async Task<string> ResguardarEnDriveAsync(ArchivoRespaldo archivo)
        {
            var cfg = ObtenerConfiguracion();
            ValidarRutas(cfg);

            string rutaLocal = Path.Combine(CarpetaVisible(cfg), archivo.Nombre);
            string nombreSubido = await _drive.SubirAsync(rutaLocal);

            cfg.UltimoArchivoSubido = nombreSubido;
            cfg.UltimaSubidaFecha = DateTime.Now;
            _config.Guardar(cfg);

            return nombreSubido;
        }

        public async Task RestaurarAsync(ArchivoRespaldo archivo)
        {
            var cfg = ObtenerConfiguracion();
            ValidarRutas(cfg);
            await _sql.RestaurarAsync(Path.Combine(cfg.RutaServidor.Trim(), archivo.Nombre));
        }

        // ---------- Auxiliares ----------

        /// <summary>Carpeta desde donde la app lee los .bak: la compartida, o la del servidor si SQL está en esta PC.</summary>
        private static string CarpetaVisible(ConfiguracionRespaldo cfg) =>
            !string.IsNullOrWhiteSpace(cfg.RutaCompartida) ? cfg.RutaCompartida.Trim() : cfg.RutaServidor.Trim();

        private static void ValidarRutas(ConfiguracionRespaldo cfg)
        {
            if (string.IsNullOrWhiteSpace(cfg.RutaServidor))
            {
                throw new InvalidOperationException(
                    "Todavía no se configuró la carpeta de respaldos. " +
                    "Abrí 'Configuración' e indicá la ruta en el servidor.");
            }
        }
    }
}
