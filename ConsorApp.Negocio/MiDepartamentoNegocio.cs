using ConsorApp.Datos;
using ConsorApp.Entidades;

namespace ConsorApp.Negocio
{
    public class MiDepartamentoInfo
    {
        public Propietario Propietario { get; set; } = null!;
        public Departamento Departamento { get; set; } = null!;
    }

    public class MiDepartamentoNegocio
    {
        private readonly PropietarioDatos _propietarioDatos = new PropietarioDatos();
        private readonly MiDepartamentoDatos _departamentoDatos = new MiDepartamentoDatos();

        public MiDepartamentoInfo? ObtenerInfo(int idUsuario)
        {
            Propietario? propietario = _propietarioDatos.ObtenerPorUsuario(idUsuario);

            if (propietario == null)
                return null;

            Departamento? departamento =
                _departamentoDatos.ObtenerPorId(propietario.IdDepartamento);

            if (departamento == null)
                return null;

            return new MiDepartamentoInfo
            {
                Propietario = propietario,
                Departamento = departamento
            };
        }
    }
}