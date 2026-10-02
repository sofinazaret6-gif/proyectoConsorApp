using System;
using System.Collections.Generic;
using ConsorApp.Datos;
using ConsorApp.Entidades;

namespace ConsorApp.Negocio
{
    public class GastoEdificioNegocio
    {
        private readonly GastoEdificioDatos _datos = new GastoEdificioDatos();

        public List<GastoEdificio> ObtenerGastos()
        {
            return _datos.ObtenerGastos();
        }

        public List<GastoEdificio> ObtenerGastosPorPeriodo(string periodo)
        {
            if (string.IsNullOrWhiteSpace(periodo))
                throw new Exception("Debe ingresar un período.");

            return _datos.ObtenerGastosPorPeriodo(periodo.Trim());
        }

        public decimal ObtenerTotalPorPeriodo(string periodo)
        {
            if (string.IsNullOrWhiteSpace(periodo))
                throw new Exception("Debe ingresar un período.");

            return _datos.ObtenerTotalPorPeriodo(periodo.Trim());
        }

        public void GuardarGasto(GastoEdificio gasto)
        {
            if (gasto == null)
                throw new Exception("El gasto no puede ser nulo.");

            gasto.descripcion = gasto.descripcion.Trim();
            gasto.periodo = gasto.periodo.Trim();
            gasto.estado = "ACTIVO";

            if (gasto.id_Edificio <= 0)
                throw new Exception("No se encontró el edificio.");

            if (gasto.Id_conceptoGasto <= 0)
                throw new Exception("Debe seleccionar un concepto de gasto.");

            if (gasto.monto <= 0)
                throw new Exception("El monto debe ser mayor a cero.");

            if (string.IsNullOrWhiteSpace(gasto.periodo))
                throw new Exception("Debe ingresar el período.");

            if (gasto.periodo.Length > 7)
                throw new Exception("El período debe tener como máximo 7 caracteres. Ejemplo: 10/2026.");

            if (gasto.descripcion.Length > 300)
                throw new Exception("La descripción no puede superar los 300 caracteres.");

            if (gasto.fecha == DateTime.MinValue)
                gasto.fecha = DateTime.Now;

            _datos.InsertarGasto(gasto);
        }

        public void AnularGasto(int idGasto)
        {
            if (idGasto <= 0)
                throw new Exception("El gasto seleccionado no es válido.");

            _datos.AnularGasto(idGasto);
        }
    }
}