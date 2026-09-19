using System.Collections.Generic;

namespace ConsorApp.Entidades
{
    public class EDIFICIO
    {
        public int id_edificio { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public int CantDepto { get; set; }
        public int CantPisos { get; set; }
        public string Ubicacion { get; set; } = string.Empty;

        public ICollection<EspacioComun> EspaciosComunes { get; set; } = new List<EspacioComun>();
        public ICollection<Departamento> Departamentos { get; set; } = new List<Departamento>();
        public ICollection<GastoEdificio> GastosEdificio { get; set; } = new List<GastoEdificio>();
    }
}