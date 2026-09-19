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

    // =========================================================
    // PAGINADO
    // =========================================================

    private int _paginaActual = 1;

        private const int ClientesPorPagina = 20;

        private int _totalClientes = 0;

        private int _totalPaginas = 1;


        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public ClientesView()
        {
            InitializeComponent();

            CargarDatosClientes();
        }


        // =========================================================
        // CARGA DE CLIENTES
        // =========================================================

        /// <summary>
        /// Carga los clientes aplicando los filtros actuales
        /// y respetando el paginado.
        /// </summary>
        public void CargarDatosClientes()
        {
            AplicarFiltros();
        }


        // =========================================================
        // OBTENER FILTROS ACTUALES
        // =========================================================

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

                    _totalClientes = 0;
                    _totalPaginas = 1;

                    ActualizarControlesPaginado();

                    return;
                }


                // ---------------------------------------------
                // OBTENER TOTAL
                // ---------------------------------------------

                _totalClientes =
                    _clienteRepository.GetTotalClientesFiltrados(
                        texto,
                        buscarPor,
                        activo,
                        fechaDesde,
                        fechaHasta);


                // ---------------------------------------------
                // CALCULAR TOTAL DE PÁGINAS
                // ---------------------------------------------

                _totalPaginas =
                    _totalClientes == 0
                        ? 1
                        : (int)Math.Ceiling(
                            (double)_totalClientes /
                            ClientesPorPagina);


                // ---------------------------------------------
                // CORREGIR PÁGINA SI ES NECESARIO
                // ---------------------------------------------

                if (_paginaActual > _totalPaginas)
                {
                    _paginaActual = _totalPaginas;
                }

                if (_paginaActual < 1)
                {
                    _paginaActual = 1;
                }


                // ---------------------------------------------
                // OBTENER CLIENTES DE LA PÁGINA
                // ---------------------------------------------

                List<Cliente> clientes =
                    _clienteRepository.GetClientesFiltradosPaginados(
                        texto,
                        buscarPor,
                        activo,
                        fechaDesde,
                        fechaHasta,
                        _paginaActual,
                        ClientesPorPagina);


                ClientesDataGrid.ItemsSource =
                    clientes;


                // ---------------------------------------------
                // ACTUALIZAR CONTROLES
                // ---------------------------------------------

                ActualizarControlesPaginado();
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
        // ACTUALIZAR PAGINADO
        // =========================================================

        private void ActualizarControlesPaginado()
        {
            TxtPaginaActual.Text =
                $"Página {_paginaActual} de {_totalPaginas}";

            BtnPaginaAnterior.IsEnabled =
                _paginaActual > 1;

            BtnPaginaSiguiente.IsEnabled =
                _paginaActual < _totalPaginas;
        }


        // =========================================================
        // CAMBIAR DE PÁGINA
        // =========================================================

        private void BtnPaginaAnterior_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (_paginaActual > 1)
            {
                _paginaActual--;

                AplicarFiltros();
            }
        }


        private void BtnPaginaSiguiente_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (_paginaActual < _totalPaginas)
            {
                _paginaActual++;

                AplicarFiltros();
            }
        }


        


        // =========================================================
        // EVENTOS DEL BUSCADOR
        // =========================================================

        private void TxtBuscar_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            if (!IsInitialized)
                return;

            _paginaActual = 1;

            AplicarFiltros();
        }


        private void CmbBuscarPor_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (!IsInitialized)
                return;

            _paginaActual = 1;

            AplicarFiltros();
        }


        private void FiltroCambiado(
            object sender,
            RoutedEventArgs e)
        {
            if (!IsInitialized)
                return;

            _paginaActual = 1;

            AplicarFiltros();
        }


        private void Fecha_SelectedDateChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (!IsInitialized)
                return;

            _paginaActual = 1;

            AplicarFiltros();
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

            _paginaActual = 1;

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
