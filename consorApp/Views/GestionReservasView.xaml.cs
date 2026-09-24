using System;
using System.Data;
using System.Windows;

namespace ConsorApp.Views
{
    public partial class GestionReservasAdminView : Window
    {
        public GestionReservasAdminView()
        {
            InitializeComponent();
            CargarReservasEjemplo();
        }

        private void CargarReservasEjemplo()
        {
            // Creamos un DataTable temporal con datos de ejemplo
            DataTable dt = new DataTable();
            dt.Columns.Add("IdReserva", typeof(int));
            dt.Columns.Add("Espacio", typeof(string));
            dt.Columns.Add("Unidad", typeof(string));
            dt.Columns.Add("Responsable", typeof(string));
            dt.Columns.Add("Fecha", typeof(string));
            dt.Columns.Add("Turno", typeof(string));
            dt.Columns.Add("Estado", typeof(string));

            // Agregamos filas de ejemplo
            dt.Rows.Add(1, "SUM", "Unidad 101", "Octavio Dorrego", "10/10/2026", "Noche (21:00 a 02:00)", "Confirmada");
            dt.Rows.Add(2, "Quincho con Parrilla", "Unidad 204", "María Gómez", "12/10/2026", "Tarde (15:00 a 20:00)", "Confirmada");
            dt.Rows.Add(3, "Piscina / Solarium", "Unidad 302", "Carlos Pérez", "15/10/2026", "Mañana (09:00 a 14:00)", "Confirmada");

            // Asignamos los datos de prueba al DataGrid
            DgReservasAdmin.ItemsSource = dt.DefaultView;
        }
    }
}