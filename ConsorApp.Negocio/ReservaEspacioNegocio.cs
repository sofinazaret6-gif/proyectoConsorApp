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

        // Actualiza automáticamente las reservas vencidas al estado "Finalizada"
        public void ActualizarReservasFinalizadas()
        {
            _datos.ActualizarReservasVencidas();
        }

        // Valida y registra una nueva reserva
        public void GuardarReserva(int idEspacioComun, int idPropietario, int idTurno, DateTime fechaReserva, string codigoSeguimiento)
        {
            if (fechaReserva.Date < DateTime.Now.Date)
            {
                throw new Exception("No se puede realizar una reserva en una fecha pasada.");
            }

            // Consultamos si hay un mensaje de bloqueo
            string detalleBloqueo = _datos.VerificarDetalleBloqueo(idEspacioComun, fechaReserva);
            if (detalleBloqueo != null)
            {
                // Si hay bloqueo, cortamos la ejecución y enviamos el detalle a la vista
                throw new Exception(detalleBloqueo);
            }

            _datos.RegistrarReserva(idEspacioComun, idPropietario, idTurno, fechaReserva, codigoSeguimiento);
        }

        // Cancela una reserva recibiendo un motivo opcional de forma segura
        public void CancelarReserva(int idReserva, string? motivo = null)
        {
            if (idReserva <= 0)
            {
                throw new Exception("Seleccione una reserva válida para cancelar.");
            }

            // Limpiamos y normalizamos el motivo de cancelación
            string? motivoLimpio = string.IsNullOrWhiteSpace(motivo) ? null : motivo.Trim();

            _datos.CancelarReserva(idReserva, motivoLimpio);
        }

        public DataTable ObtenerReservasPorPropietario(int idPropietario)
        {
            return _datos.ObtenerReservasPorPropietario(idPropietario);
        }

        // --- MÉTODOS PARA LA GESTIÓN DE INCIDENTES ---

        public DataTable ObtenerIncidentesAdmin()
        {
            return _datos.ObtenerIncidentesAdmin();
        }

        public void RegistrarIncidente(int idReserva, string descripcion)
        {
            if (idReserva <= 0)
            {
                throw new Exception("Debe seleccionar una reserva válida para asociar el incidente.");
            }

            if (string.IsNullOrWhiteSpace(descripcion))
            {
                throw new Exception("Debe ingresar una descripción detallada del daño o incidente.");
            }

            _datos.InsertarIncidente(idReserva, descripcion.Trim());
        }

        public void MarcarIncidenteSolucionado(int idIncidente)
        {
            if (idIncidente <= 0)
            {
                throw new Exception("Seleccione un incidente válido.");
            }

            _datos.ActualizarEstadoIncidente(idIncidente, "Solucionado");
        }

        public DataTable ObtenerReservasFinalizadas()
        {
            return _datos.ObtenerReservasFinalizadas();
        }
    }
}