using System;

namespace ConsorApp.Entidades
{
    /// <summary>Un archivo .bak listado en la ventana de respaldos.</summary>
    public class ArchivoRespaldo
    {
        public string Nombre { get; set; } = "";
        public DateTime Fecha { get; set; }
        public long TamanoBytes { get; set; }

        public string TamanoTexto
        {
            get
            {
                double mb = TamanoBytes / 1024.0 / 1024.0;
                return mb >= 1024
                    ? $"{mb / 1024.0:N2} GB"
                    : $"{mb:N2} MB";
            }
        }
    }
}
