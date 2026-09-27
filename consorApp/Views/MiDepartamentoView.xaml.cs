using System.Data;
using System.Windows;

namespace consorApp.Views
{
    public partial class MiDepartamentoView : Window
    {
        public MiDepartamentoView()
        {
            InitializeComponent();
            CargarDatosEjemplo();
        }

        private void CargarDatosEjemplo()
        {
            // 1. Cargar Reservas de ejemplo
            DataTable dtReservas = new DataTable();
            dtReservas.Columns.Add("Espacio", typeof(string));
            dtReservas.Columns.Add("Fecha", typeof(string));
            dtReservas.Columns.Add("Turno", typeof(string));
            dtReservas.Columns.Add("Estado", typeof(string));

            dtReservas.Rows.Add("SUM (Salón de Usos Múltiples)", "10/10/2026", "Noche (21:00 a 02:00)", "Confirmada");
            dtReservas.Rows.Add("Quincho con Parrilla", "18/10/2026", "Tarde (15:00 a 20:00)", "Cancelada");

            DgMisReservas.ItemsSource = dtReservas.DefaultView;

            // 2. Cargar Reclamos de ejemplo con la columna Observacion
            DataTable dtReclamos = new DataTable();
            dtReclamos.Columns.Add("Motivo", typeof(string));
            dtReclamos.Columns.Add("Ubicacion", typeof(string));
            dtReclamos.Columns.Add("Fecha", typeof(string));
            dtReclamos.Columns.Add("Estado", typeof(string));
            dtReclamos.Columns.Add("Observacion", typeof(string));

            dtReclamos.Rows.Add("Gotera / Filtración", "Techo Balcón / Living", "24/09/2026", "En Resolución", "El plomero pasará mañana por la tarde.");
            dtReclamos.Rows.Add("Ruidos molestos", "Departamento 5B", "20/09/2026", "Resuelto", "Se habló con el propietario del depto.");

            DgMisReclamos.ItemsSource = dtReclamos.DefaultView;
        }
    }
}