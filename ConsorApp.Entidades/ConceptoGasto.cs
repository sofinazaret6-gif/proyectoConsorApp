using System.Collections.Generic;

namespace ConsorApp.Entidades
{
    public class conceptoGasto
    {
        public int id_conceptoGasto { get; set; }
        public string nombreConcepto { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;

        public ICollection<GastoEdificio> GastosEdificio { get; set; } = new List<GastoEdificio>();
    }
}