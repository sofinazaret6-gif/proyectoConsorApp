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
        public Perfil? Perfil { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string UsuarioSistema { get; set; } = string.Empty;
        public string Contrasenia { get; set; } = string.Empty;
    }
}