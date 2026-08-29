using System;
using System.Collections.Generic;
using System.Text;
using ConsorApp.Datos;
using ConsorApp.Entidades;

namespace ConsorApp.Negocio
{
    public class UsuarioNegocio
    {
        private UsuarioDatos datos = new UsuarioDatos();

        public Usuario? IniciarSesion(string usuario, string password)
        {
            return datos.ValidarCredenciales(usuario, password);
        }
    }
}
