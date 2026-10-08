using System;
using System.Collections.Generic;
using ConsorApp.Datos;
using ConsorApp.Entidades;

namespace ConsorApp.Negocio
{
    public class HistorialExpensaNegocio
    {
        private readonly HistorialExpensaDatos _datos;

        public HistorialExpensaNegocio()
        {
            _datos = new HistorialExpensaDatos();
        }


        // =========================================================
        // OBTENER TODO EL HISTORIAL
        // =========================================================

        public List<HistorialExpensa> ObtenerHistorial()
        {
            try
            {
                return _datos.ObtenerHistorial();
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Error en la capa de negocio al obtener el historial de expensas: "
                    + ex.Message);
            }
        }


        // =========================================================
        // OBTENER HISTORIAL POR DEPARTAMENTO
        // =========================================================

        public List<HistorialExpensa> ObtenerHistorialPorDepartamento(
            int idDepartamento)
        {
            if (idDepartamento <= 0)
            {
                throw new ArgumentException(
                    "El departamento seleccionado no es válido.");
            }

            try
            {
                return _datos
                    .ObtenerHistorialPorDepartamento(
                        idDepartamento);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Error en la capa de negocio al obtener el historial del departamento: "
                    + ex.Message);
            }
        }

        public List<HistorialExpensa> ObtenerHistorialPorUsuario(int idUsuario)
        {
            return _datos.ObtenerHistorialPorUsuario(idUsuario);
        }
    }
}