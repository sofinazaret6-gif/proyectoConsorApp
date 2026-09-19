using System;

namespace ConsorApp.Entidades
{
    public class GastoEdificio
    {
        public int IdGasto { get; set; }
        public int IdEdificio { get; set; }
        public int IdConceptoGasto { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string Periodo { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string Estado { get; set; } = string.Empty;

        public EDIFICIO? Edificio { get; set; }
        public conceptoGasto? ConceptoGasto { get; set; }
    }
}