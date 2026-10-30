using System;
using System.Text.Json.Serialization;

namespace ConsorApp.Entidades
{
    /// <summary>
    /// Configuración de respaldos. Se guarda por PC en %AppData%\ConsorApp\respaldos.json
    /// (cada integrante del equipo tiene su propio SQL Server, así que no va en la base).
    /// </summary>
    public class ConfiguracionRespaldo
    {
        /// <summary>Se toma de la cadena de conexión del ApplicationDbContext; no se guarda.</summary>
        [JsonIgnore]
        public string BaseDatos { get; set; } = "";

        /// <summary>Carpeta (vista desde el equipo donde corre SQL Server) donde SQL escribe el .bak.</summary>
        public string RutaServidor { get; set; } = "";

        /// <summary>Carpeta que usa la app para listar y subir los .bak. Si está vacía se usa RutaServidor.</summary>
        public string RutaCompartida { get; set; } = "";

        public string UltimoArchivoSubido { get; set; } = "";

        public DateTime? UltimaSubidaFecha { get; set; }
    }
}
