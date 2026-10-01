using System;
using System.Collections.Generic;
using ConsorApp.Datos;
using ConsorApp.Entidades;

namespace ConsorApp.Negocio
{
    public class AvisoNegocio
    {
        private readonly AvisoDatos _avisoDatos;

        public AvisoNegocio()
        {
            _avisoDatos = new AvisoDatos();
        }


        // =========================================================
        // OBTENER TODOS LOS AVISOS
        // =========================================================

        public List<Aviso> ObtenerAvisos()
        {
            try
            {
                return _avisoDatos.ObtenerAvisos();
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Error en la capa de negocio al obtener los avisos: "
                    + ex.Message);
            }
        }


        // =========================================================
        // OBTENER AVISOS DEL PROPIETARIO
        // =========================================================

        public List<Aviso> ObtenerAvisosParaPropietario(int idUsuario)
        {
            if (idUsuario <= 0)
            {
                throw new ArgumentException(
                    "El usuario no es válido.");
            }

            try
            {
                return _avisoDatos
                    .ObtenerAvisosParaPropietario(idUsuario);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Error en la capa de negocio al obtener los avisos del propietario: "
                    + ex.Message);
            }
        }


        // =========================================================
        // REGISTRAR AVISO
        // =========================================================

        public void RegistrarAviso(
            string titulo,
            string mensaje,
            int idUsuario,
            int? idEdificio,
            int? idDepartamento)
        {
            // -----------------------------------------------------
            // VALIDAR TÍTULO
            // -----------------------------------------------------

            if (string.IsNullOrWhiteSpace(titulo))
            {
                throw new ArgumentException(
                    "El título del aviso no puede estar vacío.");
            }

            titulo = titulo.Trim();

            if (titulo.Length > 100)
            {
                throw new ArgumentException(
                    "El título del aviso no puede superar los 100 caracteres.");
            }


            // -----------------------------------------------------
            // VALIDAR MENSAJE
            // -----------------------------------------------------

            if (string.IsNullOrWhiteSpace(mensaje))
            {
                throw new ArgumentException(
                    "El mensaje del aviso no puede estar vacío.");
            }

            mensaje = mensaje.Trim();

            if (mensaje.Length > 500)
            {
                throw new ArgumentException(
                    "El mensaje del aviso no puede superar los 500 caracteres.");
            }


            // -----------------------------------------------------
            // VALIDAR USUARIO
            // -----------------------------------------------------

            if (idUsuario <= 0)
            {
                throw new ArgumentException(
                    "El usuario que publica el aviso no es válido.");
            }


            // -----------------------------------------------------
            // VALIDAR EDIFICIO
            // -----------------------------------------------------

            // La aplicación trabaja con un solo edificio.
            // Por eso todo aviso debe tener un edificio asociado.

            if (!idEdificio.HasValue || idEdificio.Value <= 0)
            {
                throw new ArgumentException(
                    "El edificio del aviso no es válido.");
            }


            // -----------------------------------------------------
            // VALIDAR DEPARTAMENTO
            // -----------------------------------------------------

            if (idDepartamento.HasValue &&
                idDepartamento.Value <= 0)
            {
                throw new ArgumentException(
                    "El departamento seleccionado no es válido.");
            }


            // -----------------------------------------------------
            // VALIDAR DESTINATARIO
            // -----------------------------------------------------

            // Aviso general:
            // idEdificio = edificio
            // idDepartamento = NULL
            //
            // Aviso particular:
            // idEdificio = edificio
            // idDepartamento = departamento


            // -----------------------------------------------------
            // CREAR AVISO
            // -----------------------------------------------------

            var nuevoAviso = new Aviso
            {
                titulo = titulo,
                mensaje = mensaje,
                fechaPublicacion = DateTime.Now,

                id_usuario = idUsuario,

                id_edificio = idEdificio,
                id_Departamento = idDepartamento,

                // 1 = vigente
                // 0 = archivado
                estado = 1
            };


            // -----------------------------------------------------
            // GUARDAR AVISO
            // -----------------------------------------------------

            try
            {
                _avisoDatos.InsertarAviso(nuevoAviso);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Error en la capa de negocio al registrar el aviso: "
                    + ex.Message);
            }
        }


        // =========================================================
        // ARCHIVAR AVISO
        // =========================================================

        public void ArchivarAviso(int idAviso)
        {
            // El ID debe ser válido.

            if (idAviso <= 0)
            {
                throw new ArgumentException(
                    "ID de aviso no válido.");
            }

            try
            {
                // Esto NO elimina el registro de la BD.
                // Cambia estado de 1 a 0.

                _avisoDatos.ArchivarAviso(idAviso);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Error en la capa de negocio al archivar el aviso: "
                    + ex.Message);
            }
        }
    }
}