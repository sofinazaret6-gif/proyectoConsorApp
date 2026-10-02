using System;
using System.Windows;
using ConsorApp.Entidades;
using ConsorApp.Negocio;

namespace consorApp.Views
{
    public partial class NuevoConceptoGastoView : Window
    {
        private readonly ConceptoGastoNegocio _conceptoNegocio =
            new ConceptoGastoNegocio();

        public NuevoConceptoGastoView()
        {
            InitializeComponent();
        }

        private void BtnGuardarConcepto_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                conceptoGasto concepto = new conceptoGasto
                {
                    nombreConcepto = TxtNombreConcepto.Text.Trim(),
                    descripcion = TxtDescripcionConcepto.Text.Trim()
                };

                _conceptoNegocio.GuardarConcepto(concepto);

                MessageBox.Show(
                    "El concepto de gasto fue guardado correctamente.",
                    "Éxito",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }
    }
}