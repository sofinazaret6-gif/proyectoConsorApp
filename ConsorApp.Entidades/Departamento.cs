using System.Collections.Generic;

namespace ConsorApp.Entidades
{
    public class Departamento
    {
        public int IdDepartamento { get; set; }
        public int IdEdificio { get; set; }
        public string Piso { get; set; } = string.Empty;
        public string Unidad { get; set; } = string.Empty;

        public EDIFICIO? Edificio { get; set; }
        public ICollection<Propietario> Propietarios { get; set; } = new List<Propietario>();
    }
}