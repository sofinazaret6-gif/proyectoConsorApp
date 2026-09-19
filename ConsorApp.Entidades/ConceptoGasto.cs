using System.Collections.Generic;

namespace ConsorApp.Entidades
{
    public class conceptoGasto
    {
        public int IdConceptoGasto { get; set; }
        public string NombreConcepto { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;

        public ICollection<GastoEdificio> GastosEdificio { get; set; } = new List<GastoEdificio>();
    }
}