using System.Collections.Generic;

namespace ConsorApp.Entidades
{
    public class Departamento
    {
        public int IdDepartamento { get; set; }
        public int IdEdificio { get; set; }
        public int IdPropietario { get; set; }
        public string Piso { get; set; } = string.Empty;
        public string Unidad { get; set; } = string.Empty;

        public EDIFICIO? Edificio { get; set; }
        public Propietario? Propietario { get; set; }
    }
}