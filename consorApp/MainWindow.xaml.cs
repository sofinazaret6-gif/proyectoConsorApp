using System;
using System.Linq;
using System.Windows;
using ConsorApp.Negocio;
using consorApp.Views;

namespace consorApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly AvisoNegocio _avisoNegocio;

        public MainWindow()
        {
            InitializeComponent();

            _avisoNegocio = new AvisoNegocio();

            CargarAvisosRecientes();
        }

        private void CargarAvisosRecientes()
        {
            try
            {
                // Obtenemos todos los avisos.
                var todosLosAvisos =
                    _avisoNegocio.ObtenerAvisos();

                // En la pantalla principal solamente
                // mostramos avisos generales del edificio.
                //
                // Un aviso general tiene:
                // id_edificio = ID del edificio
                // id_Departamento = NULL
                //
                // Además, tomamos solamente los 3 más recientes.
                var avisosGenerales =
                    todosLosAvisos
                        .Where(a => a.estado==1 &&
                            a.id_edificio.HasValue &&
                            !a.id_Departamento.HasValue)
                        .Take(3)
                        .ToList();

                ItemsAvisosRecientes.ItemsSource =
                    avisosGenerales;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al cargar los avisos recientes: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void BtnIniciarSesion_Click(
            object sender,
            RoutedEventArgs e)
        {
            // Abrir ventana de Login
            LoginView loginWindow =
                new LoginView();

            loginWindow.Show();

            // Cerrar la ventana principal
            this.Close();
        }
    }
}