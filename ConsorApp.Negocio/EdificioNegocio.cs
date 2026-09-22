using System;
using ConsorApp.Datos;
using ConsorApp.Entidades;

namespace ConsorApp.Negocio
{
    public class EdificioNegocio
    {
        private readonly EdificioDatos _datos = new EdificioDatos();

        // Obtiene el único edificio registrado
        public EDIFICIO ObtenerUnicoEdificio()
        {
            return _datos.ObtenerUnicoEdificio();
        }

        // Guardar o actualizar
        public void GuardarEdificio(EDIFICIO edificio)
        {
            ValidarEdificio(edificio);

            // Si id_edificio es 0, es la primera vez (INSERT)
            if (edificio.id_edificio == 0)
            {
                _datos.InsertarEdificio(edificio);
            }
            // Si ya tiene ID, es una actualización (UPDATE)
            else
            {
                ValidarModificacion(edificio);
                _datos.ActualizarEdificio(edificio);
            }
        }

        private static void ValidarEdificio(EDIFICIO edificio)
        {
            if (edificio == null)
                throw new ArgumentNullException(nameof(edificio), "El edificio no puede ser nulo.");

            if (string.IsNullOrWhiteSpace(edificio.Descripcion))
                throw new Exception("El nombre o descripción del edificio es obligatorio.");

            if (string.IsNullOrWhiteSpace(edificio.Ubicacion))
                throw new Exception("La ubicación o dirección del edificio es obligatoria.");

            if (edificio.CantPisos <= 0)
                throw new Exception("La cantidad de pisos debe ser mayor a 0.");

            if (edificio.CantDepto <= 0)
                throw new Exception("La cantidad de departamentos debe ser mayor a 0.");
        }

        // utiliza '_datos' para realizar consultas a la base de datos
        private void ValidarModificacion(EDIFICIO edificio)
        {
            int cantidadDepartamentos = _datos.ObtenerCantidadDepartamentos(edificio.id_edificio);

            if (edificio.CantDepto < cantidadDepartamentos)
            {
                throw new Exception(
                    $"No se puede reducir la cantidad de departamentos a {edificio.CantDepto} " +
                    $"porque actualmente hay {cantidadDepartamentos} departamentos registrados.");
            }

            int pisoMaximo = _datos.ObtenerPisoMaximo(edificio.id_edificio);

            if (edificio.CantPisos < pisoMaximo)
            {
                throw new Exception(
                    $"No se puede reducir la cantidad de pisos a {edificio.CantPisos} " +
                    $"porque existe al menos un departamento registrado en el piso {pisoMaximo}.");
            }
        }
    }
}