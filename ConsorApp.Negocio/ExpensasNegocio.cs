using System;
using System.Collections.Generic;
using ConsorApp.Datos;
using ConsorApp.Entidades;

namespace ConsorApp.Negocio
{
    public class ExpensasNegocio
    {
        private readonly ExpensasDatos _datos;

        public ExpensasNegocio()
        {
            _datos = new ExpensasDatos();
        }


        // ============================================================
        // OBTENER EXPENSAS
        // ============================================================

        public List<Expensa> ObtenerExpensas()
        {
            try
            {
                return _datos.ObtenerExpensas();
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Error al obtener las expensas: "
                    + ex.Message);
            }
        }


        // ============================================================
        // OBTENER POR PERÍODO
        // ============================================================

        public Expensa? ObtenerPorPeriodo(
            string periodo)
        {
            if (string.IsNullOrWhiteSpace(periodo))
            {
                throw new ArgumentException(
                    "El período no puede estar vacío.");
            }

            try
            {
                return _datos.ObtenerPorPeriodo(
                    periodo.Trim());
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Error al buscar la expensa: "
                    + ex.Message);
            }
        }


        // ============================================================
        // DEPARTAMENTOS CON PROPIETARIO
        // ============================================================

        public List<Departamento>
            ObtenerDepartamentosConPropietario()
        {
            try
            {
                return _datos
                    .ObtenerDepartamentosConPropietario();
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Error al obtener los departamentos con propietario: "
                    + ex.Message);
            }
        }


        // ============================================================
        // GUARDAR LIQUIDACIÓN COMPLETA
        // ============================================================

        public int GuardarLiquidacionCompleta(
            Expensa expensa,
            List<int> departamentos,
            DateTime fechaVencimiento)
        {
            if (expensa == null)
            {
                throw new ArgumentNullException(
                    nameof(expensa));
            }

            if (expensa.Id_Edificio <= 0)
            {
                throw new ArgumentException(
                    "El edificio no es válido.");
            }

            if (string.IsNullOrWhiteSpace(
                expensa.Periodo))
            {
                throw new ArgumentException(
                    "El período no puede estar vacío.");
            }

            if (expensa.MontoTotalGastos <= 0)
            {
                throw new ArgumentException(
                    "El monto total de gastos debe ser mayor a cero.");
            }

            if (expensa.ValorExpensaIndividual <= 0)
            {
                throw new ArgumentException(
                    "El valor individual debe ser mayor a cero.");
            }

            if (departamentos == null ||
                departamentos.Count == 0)
            {
                throw new ArgumentException(
                    "No existen departamentos con propietario para generar la liquidación.");
            }

            if (fechaVencimiento < expensa.FechaEmision)
            {
                throw new ArgumentException(
                    "La fecha de vencimiento no puede ser anterior a la fecha de emisión.");
            }

            // Evitamos dos liquidaciones del mismo período.
            Expensa? existente =
                _datos.ObtenerPorPeriodo(
                    expensa.Periodo.Trim());

            if (existente != null)
            {
                throw new ArgumentException(
                    "Ya existe una liquidación para ese período.");
            }

            try
            {
                return _datos.GuardarLiquidacionCompleta(
                    expensa,
                    departamentos,
                    fechaVencimiento);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Error al guardar la liquidación completa: "
                    + ex.Message);
            }
        }


        // ============================================================
        // OBTENER DETALLES
        // ============================================================

        public List<Detalle_Expensa> ObtenerDetalles(
            int idExpensa)
        {
            if (idExpensa <= 0)
            {
                throw new ArgumentException(
                    "La expensa seleccionada no es válida.");
            }

            try
            {
                return _datos.ObtenerDetalles(
                    idExpensa);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Error al obtener los detalles: "
                    + ex.Message);
            }
        }


        // ============================================================
        // REGISTRAR PAGO
        // ============================================================

        public void RegistrarPago(
            int idDetalle,
            DateTime fechaPago,
            string metodoPago)
        {
            if (idDetalle <= 0)
            {
                throw new ArgumentException(
                    "El detalle de expensa no es válido.");
            }

            if (string.IsNullOrWhiteSpace(
                metodoPago))
            {
                throw new ArgumentException(
                    "Debe indicar el método de pago.");
            }

            try
            {
                _datos.ActualizarPago(
                    idDetalle,
                    "Pagado",
                    fechaPago,
                    metodoPago.Trim());
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Error al registrar el pago: "
                    + ex.Message);
            }
        }

        // ============================================================
        // OBTENER TOTAL DE GASTOS DE UN PERÍODO
        // ============================================================

        public decimal ObtenerTotalGastosPorPeriodo(string periodo)
        {
            if (string.IsNullOrWhiteSpace(periodo))
            {
                throw new ArgumentException(
                    "El período no puede estar vacío.");
            }

            try
            {
                return _datos.ObtenerTotalGastosPorPeriodo(
                    periodo.Trim());
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Error al obtener el total de gastos del período: "
                    + ex.Message);
            }
        }


        // ============================================================
        // OBTENER CANTIDAD DE DEPARTAMENTOS CON PROPIETARIO
        // ============================================================

        public int ObtenerCantidadDepartamentosConPropietario()
        {
            try
            {
                return _datos.ObtenerCantidadDepartamentosConPropietario();
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Error al obtener la cantidad de departamentos con propietario: "
                    + ex.Message);
            }
        }
    }
}