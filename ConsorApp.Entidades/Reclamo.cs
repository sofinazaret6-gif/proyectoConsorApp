using System;

namespace ConsorApp.Entidades
{
    public class Reclamo
    {
        public int Id_Reclamo { get; set; }
        public int Id_UsuarioDepartamento { get; set; } // Apunta a IdPropietario
        public string Motivo { get; set; } = string.Empty;

        public string Ubicacion { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public string Observacion { get; set; } = string.Empty;

        public string estadoReclamo { get; set; } = string.Empty;

        public DateTime fechaReclamo { get; set; }

        public string Categoria { get; set; } = "Otro";

        public Propietario? Propietario { get; set; }
    }
}