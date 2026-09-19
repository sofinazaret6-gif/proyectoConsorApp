using System;
using System.Data;
using ConsorApp.Datos;
using ConsorApp.Entidades;

namespace ConsorApp.Negocio
{
    public class EdificioNegocio
    {
        private readonly EdificioDatos _datos =
            new EdificioDatos();

        // Obtener todos los edificios
        public DataTable ObtenerEdificios()
        {
            return _datos.ObtenerEdificios();
        }

        // Guardar o actualizar un edificio
        public void GuardarEdificio(EDIFICIO edificio)
        {
            // Validaciones generales
            ValidarEdificio(edificio);

            // NUEVO EDIFICIO
            if (edificio.id_edificio == 0)
            {
                _datos.InsertarEdificio(edificio);
            }
            // EDIFICIO EXISTENTE
            else
            {
                ValidarModificacion(edificio);

                _datos.ActualizarEdificio(edificio);
            }
        }

        // Validaciones generales
        private void ValidarEdificio(EDIFICIO edificio)
        {
            if (edificio == null)
                throw new ArgumentNullException(
                    nameof(edificio),
                    "El edificio no puede ser nulo.");

            if (string.IsNullOrWhiteSpace(
                edificio.Descripcion))
            {
                throw new Exception(
                    "El nombre o descripción del edificio es obligatorio.");
            }

            if (string.IsNullOrWhiteSpace(
                edificio.Ubicacion))
            {
                throw new Exception(
                    "La ubicación o dirección del edificio es obligatoria.");
            }

            if (edificio.CantPisos <= 0)
            {
                throw new Exception(
                    "La cantidad de pisos debe ser mayor a 0.");
            }

            if (edificio.CantDepto <= 0)
            {
                throw new Exception(
                    "La cantidad de departamentos debe ser mayor a 0.");
            }
        }

        // Validaciones cuando se modifica un edificio existente
        private void ValidarModificacion(EDIFICIO edificio)
        {
            // Cantidad actual de departamentos registrados
            int cantidadDepartamentos =
                _datos.ObtenerCantidadDepartamentos(
                    edificio.id_edificio);

            // ------------------------------------------------
            // VALIDACIÓN DE DEPARTAMENTOS
            // ------------------------------------------------

            if (edificio.CantDepto < cantidadDepartamentos)
            {
                throw new Exception(
                    $"No se puede reducir la cantidad de departamentos a " +
                    $"{edificio.CantDepto} porque actualmente hay " +
                    $"{cantidadDepartamentos} departamentos registrados.");
            }

            // ------------------------------------------------
            // VALIDACIÓN DE PISOS
            // ------------------------------------------------

            int pisoMaximo =
                _datos.ObtenerPisoMaximo(
                    edificio.id_edificio);

            if (edificio.CantPisos < pisoMaximo)
            {
                throw new Exception(
                    $"No se puede reducir la cantidad de pisos a " +
                    $"{edificio.CantPisos} porque existe al menos un " +
                    $"departamento registrado en el piso {pisoMaximo}.");
            }
        }
    }
}