
namespace consorApp.Seguridad
{
    public static class SesionUsuario
    {
        public static int IdUsuario { get; set; }
        public static int IdPerfil { get; set; }

        public static bool PuedePublicarAvisos =>
            IdPerfil == 1 || IdPerfil == 2;

        public static void CerrarSesion()
        {
            IdUsuario = 0;
            IdPerfil = 0;
        }
    }
}