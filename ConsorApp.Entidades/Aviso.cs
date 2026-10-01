using System;

namespace ConsorApp.Entidades
{
    public class Aviso
    {
        public int id_Aviso { get; set; }
        public int id_usuario { get; set; }
        public string titulo { get; set; } = string.Empty;
        public string mensaje { get; set; } = string.Empty;
        public DateTime fechaPublicacion { get; set; }
        public int? id_edificio { get; set; }
        public int? id_Departamento { get; set; }

        public int estado { get; set; } = 1;

        public Usuario? Usuario { get; set; }
    }
}