using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ConsorApp.Entidades
{
    public class Usuario
    {
        [Key]
        public int IdUsuario { get; set; }
        public int IdPerfil { get; set; }

        // Propiedad para almacenar el nombre del rol (ej: "Encargado", "Administrador")
        public string NombrePerfil { get; set; } = string.Empty;

        public Perfil? Perfil { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Dni { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UsuarioSistema { get; set; } = string.Empty;
        public string Contrasenia { get; set; } = string.Empty;
        public DateTime FechaAlta { get; set; } = DateTime.Now;

        [Range(0, 1, ErrorMessage = "El estado debe ser 0 o 1.")]
        public int Estado { get; set; } = 1;
    }
}