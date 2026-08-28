using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using consorApp.Views;

namespace consorApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void BtnIniciarSesion_Click(object sender, RoutedEventArgs e)
        {
            // Instanciar la nueva ventana de Login
            LoginView loginWindow = new LoginView();

            // Mostrar la ventana de Login
            loginWindow.Show();

            // Cerrar la ventana principal actual
            this.Close();
        }
    }
}