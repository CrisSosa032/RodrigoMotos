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
    /// Lógica de interacción para ServiciosView.xaml
    /// </summary>
    public partial class ServiciosView : UserControl
    {
        private readonly ServiciosRepository _serviciosRepository =
        new ServiciosRepository();

    // ============================================================
    // PAGINACIÓN
    // ============================================================

    private const int ServiciosPorPagina = 20;

        private int _paginaActual = 1;
        private int _totalServicios = 0;
        private int _totalPaginas = 1;

        private bool _vistaInicializada = false;

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

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
        // CARGAR DATOS
        // ============================================================

        public void CargarDatosServicios()
        {
            _paginaActual = 1;

            AplicarFiltros();
        }

        // ============================================================
        // APLICAR FILTROS Y PAGINACIÓN
        // ============================================================

        private void AplicarFiltros()
        {
            if (!_vistaInicializada)
                return;

            try
            {
                // ----------------------------------------------------
                // TEXTO DE BÚSQUEDA
                // ----------------------------------------------------

                string texto =
                    TxtBuscar.Text.Trim();

                // ----------------------------------------------------
                // CAMPO DE BÚSQUEDA
                // ----------------------------------------------------

                string buscarPor = "Nombre";

                if (CmbBuscarPor.SelectedItem
                    is ComboBoxItem item &&
                    item.Tag != null)
                {
                    buscarPor =
                        item.Tag.ToString() ?? "Nombre";
                }

                // ----------------------------------------------------
                // ESTADO
                // ----------------------------------------------------

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

                // ----------------------------------------------------
                // FECHAS
                // ----------------------------------------------------

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
                    _totalServicios = 0;
                    _totalPaginas = 1;
                    _paginaActual = 1;

                    ServiciosDataGrid.ItemsSource =
                        new List<Servicios>();

                    ActualizarControlesPaginado();

                    return;
                }

                // ----------------------------------------------------
                // OBTENER TOTAL DE SERVICIOS
                // ----------------------------------------------------

                _totalServicios =
                    _serviciosRepository.GetTotalServiciosFiltrados(
                        texto,
                        buscarPor,
                        idEstado,
                        fechaDesde,
                        fechaHasta);

                // ----------------------------------------------------
                // CALCULAR TOTAL DE PÁGINAS
                // ----------------------------------------------------

                _totalPaginas =
                    Math.Max(
                        1,
                        (int)Math.Ceiling(
                            (double)_totalServicios /
                            ServiciosPorPagina));

                // ----------------------------------------------------
                // ASEGURAR QUE LA PÁGINA ACTUAL SEA VÁLIDA
                // ----------------------------------------------------

                if (_paginaActual > _totalPaginas)
                    _paginaActual = _totalPaginas;

                if (_paginaActual < 1)
                    _paginaActual = 1;

                // ----------------------------------------------------
                // OBTENER SERVICIOS DE LA PÁGINA ACTUAL
                // ----------------------------------------------------

                List<Servicios> servicios =
                    _serviciosRepository.GetServiciosFiltradosPaginados(
                        texto,
                        buscarPor,
                        idEstado,
                        fechaDesde,
                        fechaHasta,
                        _paginaActual,
                        ServiciosPorPagina);

                ServiciosDataGrid.ItemsSource =
                    servicios;

                // ----------------------------------------------------
                // ACTUALIZAR CONTROLES DE PAGINACIÓN
                // ----------------------------------------------------

                ActualizarControlesPaginado();
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
        // ACTUALIZAR CONTROLES DE PAGINACIÓN
        // ============================================================

        private void ActualizarControlesPaginado()
        {
            TxtPaginaActual.Text =
                $"Página {_paginaActual} de {_totalPaginas}";

            BtnPaginaAnterior.IsEnabled =
                _paginaActual > 1;

            BtnPaginaSiguiente.IsEnabled =
                _paginaActual < _totalPaginas;
        }

        // ============================================================
        // EVENTOS DE FILTROS
        // ============================================================

        private void TxtBuscar_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            _paginaActual = 1;

            AplicarFiltros();
        }

        private void CmbBuscarPor_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            _paginaActual = 1;

            AplicarFiltros();
        }

        private void FiltroCambiado(
            object sender,
            RoutedEventArgs e)
        {
            _paginaActual = 1;

            AplicarFiltros();
        }

        private void Fecha_SelectedDateChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            _paginaActual = 1;

            AplicarFiltros();
        }

        // ============================================================
        // PAGINACIÓN - ANTERIOR
        // ============================================================

        private void BtnPaginaAnterior_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (_paginaActual <= 1)
                return;

            _paginaActual--;

            AplicarFiltros();
        }

        // ============================================================
        // PAGINACIÓN - SIGUIENTE
        // ============================================================

        private void BtnPaginaSiguiente_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (_paginaActual >= _totalPaginas)
                return;

            _paginaActual++;

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

            _paginaActual = 1;

            AplicarFiltros();
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
