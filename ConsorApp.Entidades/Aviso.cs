using System;

namespace ConsorApp.Entidades
{
    public class Aviso
    {
        public int IdAviso { get; set; }
        public int IdUsuario { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public DateTime FechaPublicacion { get; set; }

        public Usuario? Usuario { get; set; }
    }
}
