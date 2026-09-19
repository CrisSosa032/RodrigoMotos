using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;
using WpfNavigationProject.DataAccess;
using WpfNavigationProject.Models;

namespace WpfNavigationProject.Views
{
    /// <summary>
    /// Lógica de interacción para MotosView.xaml
    /// </summary>
    public partial class MotosView : UserControl
    {
        // ============================================================
        // VARIABLES
        // ============================================================

        private readonly MotoRepository _motoRepository =
            new MotoRepository();

        // Evita que los eventos de los filtros
        // se ejecuten durante la construcción de la vista.
        private bool _vistaInicializada = false;


        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public MotosView()
        {
            InitializeComponent();

            // A partir de este momento los controles
            // de la vista ya fueron inicializados.
            _vistaInicializada = true;

            CargarDatosMotos();
        }


        // ============================================================
        // CARGAR DATOS
        // ============================================================

        /// <summary>
        /// Carga todas las motos al iniciar la vista.
        /// </summary>
        public void CargarDatosMotos()
        {
            try
            {
                List<Moto> motos =
                    _motoRepository.GetAllMotos();

                MotosDataGrid.ItemsSource = motos;
            }
            catch (System.Configuration.ConfigurationErrorsException ex)
            {
                MessageBox.Show(
                    $"Error de configuración: Revisa tu App.config.\n\n{ex.Message}",
                    "Error de Configuración",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (SqlException ex)
            {
                MessageBox.Show(
                    $"Error de base de datos: Asegúrate de que SQL Server esté corriendo.\n\n{ex.Message}",
                    "Error SQL",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ocurrió un error inesperado:\n\n{ex.Message}",
                    "Error General",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // ============================================================
        // FILTROS
        // ============================================================

        /// <summary>
        /// Aplica los filtros de búsqueda y fechas seleccionados.
        /// </summary>
        private void AplicarFiltros()
        {
            // Evita ejecutar los filtros mientras
            // la vista todavía se está inicializando.
            if (!_vistaInicializada)
                return;

            try
            {
                string texto =
                    TxtBuscar.Text.Trim();

                string campoBusqueda =
                    ObtenerCampoBusqueda();

                DateTime? fechaDesde =
                    DpFechaDesde.SelectedDate;

                DateTime? fechaHasta =
                    DpFechaHasta.SelectedDate;


                // ====================================================
                // VALIDACIÓN DE FECHAS
                // ====================================================

                if (fechaDesde.HasValue &&
                    fechaHasta.HasValue &&
                    fechaDesde.Value.Date > fechaHasta.Value.Date)
                {
                    MotosDataGrid.ItemsSource =
                        new List<Moto>();

                    return;
                }


                // ====================================================
                // CONSULTA FILTRADA
                // ====================================================

                List<Moto> motos =
                    _motoRepository.GetMotosFiltradas(
                        texto,
                        campoBusqueda,
                        fechaDesde,
                        fechaHasta);

                MotosDataGrid.ItemsSource = motos;
            }
            catch (SqlException ex)
            {
                MessageBox.Show(
                    $"Error de base de datos al aplicar los filtros:\n\n{ex.Message}",
                    "Error SQL",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ocurrió un error al aplicar los filtros:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // ============================================================
        // OBTENER CAMPO DE BÚSQUEDA
        // ============================================================

        private string ObtenerCampoBusqueda()
        {
            if (CmbCampoBusqueda == null)
                return "Marca";

            if (CmbCampoBusqueda.SelectedItem is ComboBoxItem item)
            {
                string? contenido =
                    item.Content?.ToString();

                return contenido ?? "Marca";
            }

            return "Marca";
        }


        // ============================================================
        // EVENTOS DE FILTROS
        // ============================================================

        private void TxtBuscar_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            AplicarFiltros();
        }


        private void Filtro_Changed(
            object sender,
            SelectionChangedEventArgs e)
        {
            AplicarFiltros();
        }


        private void DpFechaDesde_SelectedDateChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            AplicarFiltros();
        }


        private void DpFechaHasta_SelectedDateChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            AplicarFiltros();
        }


        // ============================================================
        // LIMPIAR FILTROS
        // ============================================================

        private void BtnLimpiarFiltros_Click(
            object sender,
            RoutedEventArgs e)
        {
            TxtBuscar.Clear();

            CmbCampoBusqueda.SelectedIndex = 0;

            DpFechaDesde.SelectedDate = null;

            DpFechaHasta.SelectedDate = null;

            CargarDatosMotos();
        }


        // ============================================================
        // NAVEGACIÓN
        // ============================================================

        /// <summary>
        /// Centraliza la navegación al formulario
        /// de creación o edición.
        /// </summary>
        private void NavegarAFormulario(
            int idMotoAEditar)
        {
            MainWindow? mainWindow =
                Window.GetWindow(this) as MainWindow;

            if (mainWindow != null)
            {
                MotoFormView formulario =
                    new MotoFormView(idMotoAEditar);

                mainWindow.ContentFrame.Navigate(formulario);
            }
        }


        // ============================================================
        // NUEVA MOTO
        // ============================================================

        private void BtnNuevaMoto_Click(
            object sender,
            RoutedEventArgs e)
        {
            NavegarAFormulario(0);
        }


        // ============================================================
        // EDITAR MOTO
        // ============================================================

        private void BtnEditar_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is Button button &&
                button.Tag is int idMoto)
            {
                NavegarAFormulario(idMoto);
            }
        }
    }
}
