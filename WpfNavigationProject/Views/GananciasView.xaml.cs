using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using WpfNavigationProject.DataAccess;
using WpfNavigationProject.Models;

namespace WpfNavigationProject.Views
{
    public partial class GananciasView : UserControl
    {
        private readonly GananciasRepository _repository;

        // Cultura argentina para mostrar valores monetarios.
        private readonly CultureInfo _culturaArgentina =
            new CultureInfo("es-AR");

        public GananciasView()
        {
            InitializeComponent();

            _repository = new GananciasRepository();

            Loaded += GananciasView_Loaded;
        }

        private void GananciasView_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            CargarGanancias();
        }

        private void CargarGanancias()
        {
            try
            {
                // ============================================
                // OBTENER GANANCIAS
                // ============================================

                List<Ganancia> ganancias =
                    _repository.GetAllGanancias();

                DgGanancias.ItemsSource = ganancias;


                // ============================================
                // CALCULAR TOTAL
                // ============================================

                decimal total =
                    _repository.GetTotalGanancias();


                // ============================================
                // MOSTRAR TOTAL EN PESOS ARGENTINOS
                // ============================================

                TxtTotalGanancias.Text =
                    total.ToString(
                        "C2",
                        _culturaArgentina);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"No se pudieron cargar las ganancias.\n\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}