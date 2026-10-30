using System;
using System.IO;
using System.Text.Json;
using ConsorApp.Entidades;

namespace ConsorApp.Datos
{
    /// <summary>Lee y guarda la configuración de respaldos en un JSON dentro de %AppData%\ConsorApp.</summary>
    public class ConfiguracionRespaldoRepositorio
    {
        public static readonly string CarpetaDatos = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ConsorApp");

        private static readonly string ArchivoConfig = Path.Combine(CarpetaDatos, "respaldos.json");

        public ConfiguracionRespaldo Cargar()
        {
            try
            {
                if (File.Exists(ArchivoConfig))
                {
                    var cfg = JsonSerializer.Deserialize<ConfiguracionRespaldo>(File.ReadAllText(ArchivoConfig));
                    if (cfg != null) return cfg;
                }
            }
            catch (JsonException)
            {
                // Archivo dañado: se empieza de cero
            }

            return new ConfiguracionRespaldo();
        }

        public void Guardar(ConfiguracionRespaldo cfg)
        {
            Directory.CreateDirectory(CarpetaDatos);
            var opciones = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(ArchivoConfig, JsonSerializer.Serialize(cfg, opciones));
        }
    }
}
