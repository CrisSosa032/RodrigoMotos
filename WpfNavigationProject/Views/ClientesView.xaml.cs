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
    /// Lógica de interacción para ClientesView.xaml
    /// </summary>
    public partial class ClientesView : UserControl
    {
        private readonly ClienteRepository _clienteRepository =
            new ClienteRepository();


        public ClientesView()
        {
            InitializeComponent();

            CargarDatosClientes();
        }


        // =========================================================
        // CARGA INICIAL
        // =========================================================

        /// <summary>
        /// Carga los clientes activos al abrir la vista.
        /// </summary>
        public void CargarDatosClientes()
        {
            try
            {
                List<Cliente> clientes =
                    _clienteRepository.GetAllClientes();

                ClientesDataGrid.ItemsSource = clientes;
            }
            catch (System.Configuration.ConfigurationErrorsException ex)
            {
                MessageBox.Show(
                    $"Error de configuración: Revisa tu App.config.\n\n{ex.Message}",
                    "Error de Configuración");
            }
            catch (SqlException ex)
            {
                MessageBox.Show(
                    $"Error de base de datos: Asegúrate de que SQL Server esté corriendo.\n\n{ex.Message}",
                    "Error SQL");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ocurrió un error inesperado:\n\n{ex.Message}",
                    "Error General");
            }
        }


        // =========================================================
        // BUSCADOR
        // =========================================================

        /// <summary>
        /// Ejecuta la búsqueda utilizando todos los filtros actuales.
        /// </summary>
        private void AplicarFiltros()
        {
            try
            {
                // ---------------------------------------------
                // TEXTO
                // ---------------------------------------------

                string texto =
                    TxtBuscar.Text.Trim();


                // ---------------------------------------------
                // TIPO DE BÚSQUEDA
                // ---------------------------------------------

                string buscarPor = "Nombre";

                if (CmbBuscarPor.SelectedItem is ComboBoxItem item)
                {
                    buscarPor =
                        item.Content?.ToString() ?? "Nombre";
                }


                // ---------------------------------------------
                // ESTADO
                // ---------------------------------------------

                bool? activo = true;

                if (RbActivos.IsChecked == true)
                {
                    activo = true;
                }
                else if (RbInactivos.IsChecked == true)
                {
                    activo = false;
                }
                else if (RbTodos.IsChecked == true)
                {
                    activo = null;
                }


                // ---------------------------------------------
                // FECHAS
                // ---------------------------------------------

                DateTime? fechaDesde =
                    DpFechaDesde.SelectedDate;

                DateTime? fechaHasta =
                    DpFechaHasta.SelectedDate;


                // ---------------------------------------------
                // VALIDAR RANGO
                // ---------------------------------------------

                if (fechaDesde.HasValue &&
                    fechaHasta.HasValue &&
                    fechaDesde.Value.Date > fechaHasta.Value.Date)
                {
                    ClientesDataGrid.ItemsSource =
                        new List<Cliente>();

                    return;
                }


                // ---------------------------------------------
                // CONSULTA
                // ---------------------------------------------

                List<Cliente> clientes =
                    _clienteRepository.GetClientesFiltrados(
                        texto,
                        buscarPor,
                        activo,
                        fechaDesde,
                        fechaHasta);


                ClientesDataGrid.ItemsSource = clientes;
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
                    $"Ocurrió un error inesperado:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // EVENTOS DEL BUSCADOR
        // =========================================================

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
            if (IsInitialized)
            {
                AplicarFiltros();
            }
        }


        private void FiltroCambiado(
            object sender,
            RoutedEventArgs e)
        {
            if (IsInitialized)
            {
                AplicarFiltros();
            }
        }


        private void Fecha_SelectedDateChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (IsInitialized)
            {
                AplicarFiltros();
            }
        }


        // =========================================================
        // LIMPIAR FILTROS
        // =========================================================

        private void BtnLimpiarFiltros_Click(
            object sender,
            RoutedEventArgs e)
        {
            TxtBuscar.Clear();

            CmbBuscarPor.SelectedIndex = 0;

            RbActivos.IsChecked = true;

            DpFechaDesde.SelectedDate = null;
            DpFechaHasta.SelectedDate = null;

            CargarDatosClientes();
        }


        // =========================================================
        // NAVEGACIÓN
        // =========================================================

        private void NavegarAFormulario(
            int idClienteAEditar)
        {
            MainWindow? mainWindow =
                Window.GetWindow(this) as MainWindow;

            if (mainWindow != null)
            {
                ClienteFormView formulario =
                    new ClienteFormView(idClienteAEditar);

                mainWindow.ContentFrame.Navigate(formulario);
            }
        }


        // =========================================================
        // NUEVO CLIENTE
        // =========================================================

        private void BtnNuevoCliente_Click(
            object sender,
            RoutedEventArgs e)
        {
            NavegarAFormulario(0);
        }


        // =========================================================
        // EDITAR CLIENTE
        // =========================================================

        private void BtnEditar_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is Button button &&
                button.Tag is int idCliente)
            {
                NavegarAFormulario(idCliente);
            }
        }


        // =========================================================
        // DESACTIVAR CLIENTE
        // =========================================================

        private void BtnEliminar_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is Button button &&
                button.Tag is int idCliente)
            {
                MessageBoxResult resultado =
                    MessageBox.Show(
                        "¿Estás seguro de que deseas desactivar este cliente?\n\n" +
                        "Las motos y servicios asociados se conservarán " +
                        "como historial.",
                        "Confirmar desactivación",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                if (resultado == MessageBoxResult.Yes)
                {
                    try
                    {
                        _clienteRepository.DeleteCliente(
                            idCliente);

                        MessageBox.Show(
                            "Cliente desactivado correctamente.",
                            "Éxito",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

                        AplicarFiltros();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"Error al intentar desactivar:\n\n{ex.Message}",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
            }
        }


        // =========================================================
        // ACTIVAR CLIENTE
        // =========================================================

        private void BtnActivar_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is Button button &&
                button.Tag is int idCliente)
            {
                MessageBoxResult resultado =
                    MessageBox.Show(
                        "¿Deseas activar nuevamente este cliente?",
                        "Confirmar activación",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                if (resultado == MessageBoxResult.Yes)
                {
                    try
                    {
                        _clienteRepository.RestaurarCliente(
                            idCliente);

                        MessageBox.Show(
                            "Cliente activado correctamente.",
                            "Éxito",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

                        AplicarFiltros();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"Error al intentar activar:\n\n{ex.Message}",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}