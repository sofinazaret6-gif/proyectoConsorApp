using System;
using System.Data;
using ConsorApp.Datos;
using ConsorApp.Entidades;

namespace ConsorApp.Negocio
{
    /// <summary>
    /// Valida los datos recibidos de las vistas y administra
    /// el flujo de negocio antes de consultar la base de datos.
    /// </summary>
    public class DepartamentoNegocio
    {
        private readonly DepartamentoDatos _datos =
            new DepartamentoDatos();

        /// <summary>
        /// Obtiene el listado completo de departamentos.
        /// </summary>
        public DataTable ObtenerDepartamentos()
        {
            return _datos.ObtenerDepartamentos();
        }

        /// <summary>
        /// Evalúa si debe insertar o actualizar un departamento.
        /// Controla que no se supere el límite y devuelve el ID del departamento (ideal para asociar cosas).
        /// </summary>
        public int GuardarDepartamento(Departamento depto)
        {
            ValidarDepartamento(depto);

            // Si es un departamento nuevo
            if (depto.IdDepartamento == 0)
            {
                int cantidadActual =
                    _datos.ObtenerCantidadDepartamentos(
                        depto.IdEdificio);

                int cantidadMaxima =
                    _datos.ObtenerCantidadMaximaDepartamentos(
                        depto.IdEdificio);

                // Verificamos si el edificio ya llegó a su límite
                if (cantidadActual >= cantidadMaxima)
                {
                    throw new Exception(
                        $"El edificio ya tiene asignados " +
                        $"{cantidadActual} de {cantidadMaxima} departamentos. " +
                        $"No se pueden agregar más departamentos.");
                }

                // Insertamos y recuperamos el ID generado por la base de datos
                return _datos.InsertarDepartamento(depto);
            }
            else
            {
                // Si es una edición, actualizamos y devolvemos su ID actual
                _datos.ActualizarDepartamento(depto);
                return depto.IdDepartamento;
            }
        }

        /// <summary>
        /// Vincula un departamento con un usuario propietario en la tabla intermedia.
        /// </summary>
        public void AsignarPropietarioADepartamento(int idDepartamento, int idUsuarioPropietario)
        {
            if (idDepartamento <= 0)
                throw new Exception("El ID del departamento no es válido para asignar propietario.");

            if (idUsuarioPropietario <= 0)
                throw new Exception("Debe seleccionar un propietario válido.");

            // Llamamos a la capa de datos para hacer el insert en la relación Propietario
            _datos.AsignarPropietario(idDepartamento, idUsuarioPropietario);
        }

        /// <summary>
        /// Aplica las reglas requeridas antes de persistir el departamento.
        /// </summary>
        private void ValidarDepartamento(Departamento depto)
        {
            if (depto == null)
                throw new ArgumentNullException(
                    nameof(depto),
                    "El departamento no puede ser nulo.");

            if (depto.IdEdificio <= 0)
                throw new Exception(
                    "Debe seleccionar un edificio válido.");

            if (string.IsNullOrWhiteSpace(depto.Piso))
                throw new Exception(
                    "El número o identificación del piso es obligatorio (ej: '1', 'PB').");

            if (string.IsNullOrWhiteSpace(depto.Unidad))
                throw new Exception(
                    "La unidad o número de departamento es obligatoria (ej: 'A', '101').");
        }
    }
}