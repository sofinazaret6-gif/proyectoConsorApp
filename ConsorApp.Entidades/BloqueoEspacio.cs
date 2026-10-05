using System;
using System.Collections.Generic;
using System.Text;

namespace ConsorApp.Entidades
{
    public class BloqueoEspacio
    {
        public int IdBloqueo { get; set; }
        public int IdEspacioComun { get; set; }
        public string NombreEspacio { get; set; } // Para mostrar en la grilla
        public DateTime FechaDesde { get; set; }
        public DateTime FechaHasta { get; set; }
        public string Motivo { get; set; }
    }
}