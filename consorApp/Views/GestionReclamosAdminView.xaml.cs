using System;
using System.Data;
using System.Windows;

namespace consorApp.Views
{
    public partial class GestionReclamosAdminView : Window
    {
        public GestionReclamosAdminView()
        {
            InitializeComponent();
            CargarReclamosEjemplo();
        }

        private void CargarReclamosEjemplo()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("IdReclamo", typeof(int));
            dt.Columns.Add("Motivo", typeof(string));
            dt.Columns.Add("Ubicacion", typeof(string));
            dt.Columns.Add("Responsable", typeof(string));
            dt.Columns.Add("Fecha", typeof(string));
            dt.Columns.Add("Estado", typeof(string));

            // Datos de prueba
            dt.Rows.Add(1, "Ruidos molestos", "Departamento 2B", "Carlos Gómez (Depto 2B)", "24/09/2026", "Pendiente");
            dt.Rows.Add(2, "Gotera / Filtración", "Palier Piso 3", "María Pérez (Depto 3A)", "23/09/2026", "En proceso");
            dt.Rows.Add(3, "Basura fuera de horario", "Planta Baja / Hall", "Encargado", "22/09/2026", "Resuelto");

            DgReclamosAdmin.ItemsSource = dt.DefaultView;
        }
    }
}