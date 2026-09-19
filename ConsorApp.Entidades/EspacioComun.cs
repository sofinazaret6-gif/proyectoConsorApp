using System;
using System.Collections.Generic;
using System.Text;

namespace ConsorApp.Entidades
{
     public class EspacioComun
        {
            public int IdEspacioComun { get; set; }
            public int IdEdificio { get; set; }
            public string Nombre { get; set; } = string.Empty;
            public string Descripcion { get; set; } = string.Empty;
            public int Capacidad { get; set; }
            public string Estado { get; set; } = string.Empty;

            public EDIFICIO? Edificio { get; set; }
            public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
        }
    }

