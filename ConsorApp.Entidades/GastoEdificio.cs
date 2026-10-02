using System;

namespace ConsorApp.Entidades
{
    public class GastoEdificio
    {
        public int id_Gasto { get; set; }
        public int id_Edificio { get; set; }
        public int Id_conceptoGasto { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public string periodo { get; set; } = string.Empty;
        public DateTime fecha { get; set; }
        public string estado { get; set; } = string.Empty;

        public EDIFICIO? Edificio { get; set; }
        public conceptoGasto? ConceptoGasto { get; set; }
    }
}