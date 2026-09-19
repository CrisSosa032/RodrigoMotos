using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics.X86;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;
using WpfNavigationProject.DataAccess;
using WpfNavigationProject.Models;

namespace WpfNavigationProject.Views
{
    /// <summary>
    /// Lógica de interacción para ServiciosView.xaml
    /// </summary>
    public partial class ServiciosView : UserControl
    {
        private readonly ServiciosRepository _serviciosRepository =
            new ServiciosRepository();

        private bool _vistaInicializada = false;

        public ServiciosView()
        {
            InitializeComponent();
        }

        // ============================================================
        // AL CARGAR LA VISTA
        // ============================================================

        private void UserControl_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            if (_vistaInicializada)
                return;

            _vistaInicializada = true;

            CargarDatosServicios();
        }

        // ============================================================
        // CARGAR TODOS LOS SERVICIOS
        // ============================================================

        public void CargarDatosServicios()
        {
            try
            {
                List<Servicios> servicios =
                    _serviciosRepository.GetAllServicios();

                ServiciosDataGrid.ItemsSource = servicios;
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
                    $"Error:\n\n{ex.Message}\n\n" +
                    $"Origen:\n{ex.StackTrace}",
                    "Error General",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // ============================================================
        // APLICAR FILTROS
        // ============================================================

        private void AplicarFiltros()
        {
            if (!_vistaInicializada)
                return;

            try
            {
                string texto =
                    TxtBuscar.Text.Trim();

                string buscarPor = "Nombre";

                if (CmbBuscarPor.SelectedItem
                    is ComboBoxItem item &&
                    item.Tag != null)
                {
                    buscarPor =
                        item.Tag.ToString() ?? "Nombre";
                }

                int? idEstado = null;

                if (RbIngresado.IsChecked == true)
                {
                    idEstado = 1;
                }
                else if (RbEnProceso.IsChecked == true)
                {
                    idEstado = 2;
                }
                else if (RbTerminado.IsChecked == true)
                {
                    idEstado = 3;
                }
                else if (RbPagado.IsChecked == true)
                {
                    idEstado = 4;
                }

                DateTime? fechaDesde =
                    DpFechaDesde.SelectedDate;

                DateTime? fechaHasta =
                    DpFechaHasta.SelectedDate;

                // ----------------------------------------------------
                // VALIDAR RANGO DE FECHAS
                // ----------------------------------------------------

                if (fechaDesde.HasValue &&
                    fechaHasta.HasValue &&
                    fechaDesde.Value.Date >
                    fechaHasta.Value.Date)
                {
                    ServiciosDataGrid.ItemsSource =
                        new List<Servicios>();

                    return;
                }

                // ----------------------------------------------------
                // BUSCAR SERVICIOS FILTRADOS
                // ----------------------------------------------------

                List<Servicios> servicios =
                    _serviciosRepository.GetServiciosFiltrados(
                        texto,
                        buscarPor,
                        idEstado,
                        fechaDesde,
                        fechaHasta);

                ServiciosDataGrid.ItemsSource =
                    servicios;
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
                    $"Error de base de datos:\n\n{ex.Message}",
                    "Error SQL",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ocurrió un error inesperado:\n\n" +
                    $"{ex.Message}\n\n" +
                    $"Origen:\n{ex.StackTrace}",
                    "Error General",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
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

        private void CmbBuscarPor_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            AplicarFiltros();
        }

        private void FiltroCambiado(
            object sender,
            RoutedEventArgs e)
        {
            AplicarFiltros();
        }

        private void Fecha_SelectedDateChanged(
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

            CmbBuscarPor.SelectedIndex = 0;

            RbTodos.IsChecked = true;

            DpFechaDesde.SelectedDate = null;
            DpFechaHasta.SelectedDate = null;

            CargarDatosServicios();
        }

        // ============================================================
        // NAVEGAR AL FORMULARIO
        // ============================================================

        private void NavegarAFormulario(
            int idServicioAEditar)
        {
            MainWindow? mainWindow =
                Window.GetWindow(this) as MainWindow;

            if (mainWindow != null)
            {
                ServicioFormView formulario =
                    new ServicioFormView(
                        idServicioAEditar);

                mainWindow.ContentFrame.Navigate(
                    formulario);
            }
        }

        // ============================================================
        // NUEVO SERVICIO
        // ============================================================

        private void BtnNuevoServicio_Click(
            object sender,
            RoutedEventArgs e)
        {
            NavegarAFormulario(0);
        }

        // ============================================================
        // EDITAR SERVICIO
        // ============================================================

        private void BtnEditar_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is Button button &&
                button.Tag is int idServicio)
            {
                NavegarAFormulario(idServicio);
            }
        }
    }
}
