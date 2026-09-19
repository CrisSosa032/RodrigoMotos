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

        // Siempre mostramos 22 motos por página.
        private const int MotosPorPagina = 22;

        private int _paginaActual = 1;
        private int _totalMotos = 0;
        private int _totalPaginas = 1;

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
        /// Carga las motos aplicando los filtros actuales
        /// y utilizando paginación.
        /// </summary>
        public void CargarDatosMotos()
        {
            AplicarFiltros();
        }


        // ============================================================
        // APLICAR FILTROS Y PAGINACIÓN
        // ============================================================

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
                    _totalMotos = 0;
                    _totalPaginas = 1;
                    _paginaActual = 1;

                    MotosDataGrid.ItemsSource =
                        new List<Moto>();

                    ActualizarControlesPaginado();

                    return;
                }


                // ====================================================
                // OBTENER TOTAL DE RESULTADOS
                // ====================================================

                _totalMotos =
                    _motoRepository.GetTotalMotosFiltradas(
                        texto,
                        campoBusqueda,
                        fechaDesde,
                        fechaHasta);


                // ====================================================
                // CALCULAR TOTAL DE PÁGINAS
                // ====================================================

                _totalPaginas =
                    (int)Math.Ceiling(
                        (double)_totalMotos / MotosPorPagina);

                if (_totalPaginas < 1)
                    _totalPaginas = 1;


                // Si la página actual quedó fuera de rango
                // volvemos a la última página disponible.
                if (_paginaActual > _totalPaginas)
                    _paginaActual = _totalPaginas;

                if (_paginaActual < 1)
                    _paginaActual = 1;


                // ====================================================
                // OBTENER MOTOS DE LA PÁGINA ACTUAL
                // ====================================================

                List<Moto> motos =
                    _motoRepository.GetMotosFiltradasPaginadas(
                        texto,
                        campoBusqueda,
                        fechaDesde,
                        fechaHasta,
                        _paginaActual,
                        MotosPorPagina);

                MotosDataGrid.ItemsSource = motos;


                // ====================================================
                // ACTUALIZAR CONTROLES
                // ====================================================

                ActualizarControlesPaginado();
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
        // PÁGINA ANTERIOR
        // ============================================================

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


        // ============================================================
        // PÁGINA SIGUIENTE
        // ============================================================

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


        // ============================================================
        // OBTENER CAMPO DE BÚSQUEDA
        // ============================================================

        private string ObtenerCampoBusqueda()
        {
            if (CmbCampoBusqueda == null)
                return "Cliente";

            if (CmbCampoBusqueda.SelectedItem is ComboBoxItem item)
            {
                string? contenido =
                    item.Content?.ToString();

                return contenido ?? "Cliente";
            }

            return "Cliente";
        }


        // ============================================================
        // EVENTOS DE FILTROS
        // ============================================================

        private void TxtBuscar_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            if (!_vistaInicializada)
                return;

            _paginaActual = 1;

            AplicarFiltros();
        }


        private void Filtro_Changed(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (!_vistaInicializada)
                return;

            _paginaActual = 1;

            AplicarFiltros();
        }


        private void DpFechaDesde_SelectedDateChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (!_vistaInicializada)
                return;

            _paginaActual = 1;

            AplicarFiltros();
        }


        private void DpFechaHasta_SelectedDateChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (!_vistaInicializada)
                return;

            _paginaActual = 1;

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

            _paginaActual = 1;

            AplicarFiltros();
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