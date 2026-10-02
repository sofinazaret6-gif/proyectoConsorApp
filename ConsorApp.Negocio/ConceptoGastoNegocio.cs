using System;
using System.Collections.Generic;
using ConsorApp.Datos;
using ConsorApp.Entidades;

namespace ConsorApp.Negocio
{
    public class ConceptoGastoNegocio
    {
        private readonly ConceptoGastoDatos _datos = new ConceptoGastoDatos();

        public List<conceptoGasto> ObtenerConceptos()
        {
            return _datos.ObtenerConceptos();
        }

        public void GuardarConcepto(conceptoGasto concepto)
        {
            if (concepto == null)
                throw new Exception("El concepto no puede ser nulo.");

            concepto.nombreConcepto = concepto.nombreConcepto.Trim();
            concepto.descripcion = concepto.descripcion.Trim();

            if (string.IsNullOrWhiteSpace(concepto.nombreConcepto))
                throw new Exception("Debe ingresar un nombre para el concepto.");

            if (concepto.nombreConcepto.Length > 100)
                throw new Exception("El nombre del concepto no puede superar los 100 caracteres.");

            if (concepto.descripcion.Length > 300)
                throw new Exception("La descripción no puede superar los 300 caracteres.");

            _datos.InsertarConcepto(concepto);
        }
    }
}