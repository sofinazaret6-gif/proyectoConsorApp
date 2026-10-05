using System;
using System.Data;
using ConsorApp.Datos;

namespace ConsorApp.Negocio
{
    public class ReservaEspacioNegocio
    {
        private readonly TurnoReservaDatos _datos = new TurnoReservaDatos();

        // Obtiene los turnos para el ComboBox de la vista de reservas
        public DataTable ObtenerTurnos()
        {
            return _datos.ObtenerTurnos();
        }

        // Obtiene todas las reservas para la grilla del panel de administración
        public DataTable ObtenerReservasAdmin()
        {
            return _datos.ObtenerReservasAdmin();
        }

        // Valida y registra una nueva reserva
        public void GuardarReserva(int idEspacioComun, int idPropietario, int idTurno, DateTime fechaReserva, string codigoSeguimiento)
        {
            if (fechaReserva.Date < DateTime.Now.Date)
            {
                throw new Exception("No se puede realizar una reserva en una fecha pasada.");
            }

            // Validamos si el espacio se encuentra bloqueado por mantenimiento en esa fecha
            if (_datos.EstaEspacioBloqueo(idEspacioComun, fechaReserva))
            {
                throw new Exception("El espacio seleccionado se encuentra bloqueado por mantenimiento en la fecha indicada.");
            }

            _datos.RegistrarReserva(idEspacioComun, idPropietario, idTurno, fechaReserva, codigoSeguimiento);
        }

        public void CancelarReserva(int idReserva)
        {
            if (idReserva <= 0)
            {
                throw new Exception("Seleccione una reserva válida para cancelar.");
            }
            _datos.CancelarReserva(idReserva);
        }
    }
}