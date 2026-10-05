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
    }
}