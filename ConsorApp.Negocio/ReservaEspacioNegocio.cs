using System;
using System.Data;
using ConsorApp.Datos;

namespace ConsorApp.Negocio
{
    public class ReservaEspacioNegocio
    {
        private readonly TurnoReservaDatos _datos = new TurnoReservaDatos();
        private readonly AvisoNegocio _avisoNegocio = new AvisoNegocio(); // Instanciamos la lógica de avisos

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

            // Validación: Verificar si ya existe una reserva activa para ese espacio, turno y fecha
            if (_datos.ExisteReserva(idEspacioComun, idTurno, fechaReserva))
            {
                throw new Exception("El espacio común ya se encuentra reservado para este turno en la fecha seleccionada.");
            }

            // Consultamos si hay un mensaje de bloqueo
            string detalleBloqueo = _datos.VerificarDetalleBloqueo(idEspacioComun, fechaReserva);
            if (detalleBloqueo != null)
            {
                throw new Exception(detalleBloqueo);
            }

            _datos.RegistrarReserva(idEspacioComun, idPropietario, idTurno, fechaReserva, codigoSeguimiento);
        }

        // Cancela una reserva, la actualiza en la BD y le envía un aviso automático al propietario
        public void CancelarReserva(int idReserva, string? motivo = null)
        {
            if (idReserva <= 0)
            {
                throw new Exception("Seleccione una reserva válida para cancelar.");
            }

            // 1. Obtenemos los datos de la reserva antes de cancelarla (para saber a quién enviarle el aviso)
            var datosReserva = _datos.ObtenerDatosReservaParaAviso(idReserva);

            // 2. Limpiamos y normalizamos el motivo de cancelación
            string? motivoLimpio = string.IsNullOrWhiteSpace(motivo) ? "Sin motivo especificado" : motivo.Trim();

            // 3. Ejecutamos la cancelación en la base de datos
            _datos.CancelarReserva(idReserva, motivoLimpio);

            // 4. Si el propietario tiene un usuario asociado, le enviamos el aviso automáticamente
            if (datosReserva.idUsuario > 0)
            {
                try
                {
                    string tituloAviso = "Su reserva ha sido cancelada";
                    string mensajeAviso = $"Lamentamos informarle que su reserva para '{datosReserva.nombreEspacio}' prevista para el día {datosReserva.fechaReserva:dd/MM/yyyy} ha sido cancelada.\nMotivo: {motivoLimpio}";

                    // Registramos el aviso mediante la capa de negocio de avisos
                    _avisoNegocio.RegistrarAviso(tituloAviso, mensajeAviso, datosReserva.idUsuario, 1, datosReserva.idDepartamento);
                }
                catch
                {
                    // Si ocurre un error enviando el aviso secundario, no interrumpimos la cancelación principal
                }
            }
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