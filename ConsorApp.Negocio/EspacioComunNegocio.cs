using System.Data;
using ConsorApp.Datos;
using ConsorApp.Entidades;

namespace ConsorApp.Negocio
{
    public class EspacioComunNegocio
    {
        private readonly EspacioComunDatos _datos = new EspacioComunDatos();

        public void GuardarEspacioComun(int idEdificio, EspacioComun espacio)
        {
            _datos.InsertarEspacioComun(idEdificio, espacio);
        }

        public DataTable ObtenerEspaciosComunes()
        {
            return _datos.ObtenerEspaciosComunes();
        }

        // --- NUEVOS MÉTODOS AGREGADOS ---

        public void ActualizarEspacioComun(EspacioComun espacio)
        {
            _datos.ActualizarEspacioComun(espacio);
        }

        public DataTable ObtenerEspaciosComunesActivos()
        {
            return _datos.ObtenerEspaciosComunesActivos();
        }
    }
}