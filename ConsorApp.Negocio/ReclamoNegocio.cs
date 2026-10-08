using ConsorApp.Datos;
using ConsorApp.Entidades;
using System;
using System.Data;

namespace ConsorApp.Negocio
{
    public class ReclamoNegocio
    {
        private readonly ReclamoDatos _reclamoDatos;
        private readonly PropietarioDatos _propietarioDatos;

        public ReclamoNegocio()
        {
            _reclamoDatos = new ReclamoDatos();
            _propietarioDatos = new PropietarioDatos();
        }

        public bool RegistrarReclamo(int idUsuario, string motivo, string ubicacion, string descripcion)
        {
            if (string.IsNullOrWhiteSpace(motivo))
                throw new Exception("Ingresá o seleccioná un motivo.");

            if (string.IsNullOrWhiteSpace(ubicacion))
                throw new Exception("Ingresá la ubicación o área afectada.");

            if (string.IsNullOrWhiteSpace(descripcion))
                throw new Exception("Ingresá una descripción del problema.");

            Propietario? propietario = _propietarioDatos.ObtenerPorUsuario(idUsuario);
            if (propietario == null)
            {
                throw new Exception("No se encontró un propietario asociado al usuario actual.");
            }

            Reclamo reclamo = new Reclamo
            {
                Id_UsuarioDepartamento = propietario.IdPropietario,
                Motivo = motivo,
                Categoria = Motivos.Clasificar(motivo, descripcion),   // <-- NUEVO
                Ubicacion = ubicacion,
                Descripcion = descripcion,
                Observacion = string.Empty,
                estadoReclamo = "Pendiente",
                fechaReclamo = DateTime.Now
            };

            return _reclamoDatos.RegistrarReclamo(reclamo);
        }

        public DataTable ObtenerReclamosAdmin()
        {
            return _reclamoDatos.ObtenerReclamosParaAdmin();
        }

        public bool ActualizarEstadoYObservacion(int idReclamo, string nuevoEstado, string observacion)
        {
            if (string.IsNullOrWhiteSpace(nuevoEstado))
                throw new Exception("El estado no puede estar vacío.");

            if (string.IsNullOrWhiteSpace(observacion))
                throw new Exception("Por favor, escriba una observación o motivo para este cambio de estado.");

            return _reclamoDatos.ActualizarEstadoYObservacion(idReclamo, nuevoEstado, observacion);
        }

        public bool ActualizarObservacion(int idReclamo, string observacion)
        {
            if (string.IsNullOrWhiteSpace(observacion))
                throw new Exception("La observación no puede estar vacía.");

            return _reclamoDatos.ActualizarObservacion(idReclamo, observacion);
        }

        public DataTable ObtenerReclamosPorPropietario(int idPropietario)
        {
            return _reclamoDatos.ObtenerReclamosPorPropietario(idPropietario);
        }
    }
}