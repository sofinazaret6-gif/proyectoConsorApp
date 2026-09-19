using System;
using System.Collections.Generic;

namespace ConsorApp.Entidades
{
    public class Propietario
    {
        public int IdPropietario { get; set; }
        public int IdUsuario { get; set; }
        public int IdDepartamento { get; set; }
        public DateTime FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public string Estado { get; set; } = string.Empty;

        public Usuario? Usuario { get; set; }
        public Departamento? Departamento { get; set; }
        public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
        public ICollection<Reclamo> Reclamos { get; set; } = new List<Reclamo>();
    }
}