using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ConsorApp.Entidades
{
    public class Perfil
    {
        [Key]
        public int IdPerfil { get; set; }
        public string NombrePerfil { get; set; } = string.Empty;
    }
}
