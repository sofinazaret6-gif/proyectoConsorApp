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
        /// También controla que no se supere la cantidad máxima
        /// de departamentos permitidos por el edificio.
        /// </summary>
        public void GuardarDepartamento(Departamento depto)
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

                _datos.InsertarDepartamento(depto);
            }
            else
            {
                // Si es una edición, no controlamos el límite.
                _datos.ActualizarDepartamento(depto);
            }
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