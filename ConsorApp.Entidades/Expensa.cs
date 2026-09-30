using System;
using System.Collections.Generic;

namespace ConsorApp.Entidades
{
    public class Expensa
    {
        public int Id_Expensa { get; set; }
        public int Id_Edificio { get; set; }
        public string Periodo { get; set; } = string.Empty;
        public decimal MontoTotalGastos { get; set; }
        public decimal ValorExpensaIndividual { get; set; }
        public DateTime FechaEmision { get; set; }

        // Propiedad de navegación hacia Edificio (según tu diagrama)
        public EDIFICIO EDIFICIO { get; set; } = null!;

        // Relación 1 a muchos con los detalles por departamento
        public ICollection<Detalle_Expensa> DetallesExpensa { get; set; } = new List<Detalle_Expensa>();
    }
}