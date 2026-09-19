using System;
using System.Windows;
using System.Windows.Controls;
using WpfNavigationProject.Models;
using WpfNavigationProject.DataAccess;
using Microsoft.Data.SqlClient;

namespace WpfNavigationProject.Views
{
    public partial class ServicioFormView : UserControl
    {
        // Repositorio de servicios.
        private ServiciosRepository _repo =
            new ServiciosRepository();

        // ID del servicio que estamos editando.
        // Si es 0, significa que estamos creando uno nuevo.
        private int _idServicioActual;

        // ============================================================
        // CONSTRUCTOR NORMAL
        // ============================================================
        //
        // Se utiliza cuando:
        // - Creamos un servicio normalmente.
        // - Editamos un servicio existente.
        //
        public ServicioFormView(int idServicio)
        {
            InitializeComponent();

            _idServicioActual = idServicio;

            // Si es un servicio nuevo, mostramos la fecha actual.
            // La fecha definitiva será generada por SQL Server.
            if (_idServicioActual == 0)
            {
                DpFechaEntrada.SelectedDate = DateTime.Today;
            }

            // Si el ID es mayor a 0, estamos editando.
            if (_idServicioActual > 0)
            {
                PrellenarFormulario();

                BtnCargarServicio.Content =
                    "Actualizar Servicio";
            }
        }

        // ============================================================
        // NUEVO CONSTRUCTOR
        // ============================================================
        //
        // Se utiliza cuando acabamos de registrar una moto
        // y queremos agregarle inmediatamente un servicio.
        //
        public ServicioFormView(int idMoto, int idEstado)
        {
            InitializeComponent();

            // Estamos creando un servicio nuevo.
            _idServicioActual = 0;

            // Precargamos el ID de la moto.
            TxtIdMoto.Text = idMoto.ToString();

            // Precargamos el estado.
            TxtIdEstado.Text = idEstado.ToString();

            // Mostramos la fecha actual.
            DpFechaEntrada.SelectedDate = DateTime.Today;
        }

        private void PrellenarFormulario()
        {
            try
            {
                // Buscamos los datos en la base de datos.
                Servicios? servicio =
                    _repo.GetServicioById(_idServicioActual);

                if (servicio != null)
                {
                    // ID de la moto.
                    TxtIdMoto.Text =
                        servicio.IdMoto.ToString();

                    // ID del estado.
                    TxtIdEstado.Text =
                        servicio.IdEstado.ToString();

                    // Fecha original del servicio.
                    DpFechaEntrada.SelectedDate =
                        servicio.FechaEntrada;

                    // Costo estimado.
                    TxtCostoEstimado.Text =
                        servicio.CostoEstimado.HasValue
                            ? servicio.CostoEstimado.Value
                                .ToString("0.00")
                            : string.Empty;

                    // Detalle.
                    TxtDetalle.Text =
                        servicio.Detalle;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al cargar datos: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void BtnCargarServicio_Click(
            object sender,
            RoutedEventArgs e)
        {
            // ========================================================
            // 1. VALIDACIONES BÁSICAS
            // ========================================================

            //
            // FechaEntrada YA NO se valida porque SQL Server
            // la genera automáticamente.
            //
            if (string.IsNullOrWhiteSpace(TxtIdMoto.Text) ||
                string.IsNullOrWhiteSpace(TxtIdEstado.Text) ||
                string.IsNullOrWhiteSpace(TxtDetalle.Text))
            {
                MessageBox.Show(
                    "El ID de la Moto, el ID del Estado y el Detalle son obligatorios.",
                    "Validación",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            // Validamos que los IDs sean números enteros.
            if (!int.TryParse(
                    TxtIdMoto.Text,
                    out int idMotoValido) ||
                !int.TryParse(
                    TxtIdEstado.Text,
                    out int idEstadoValido))
            {
                MessageBox.Show(
                    "Los campos de ID (Moto y Estado) deben ser números enteros válidos.",
                    "Error de formato",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            // ========================================================
            // 2. VALIDAMOS EL COSTO ESTIMADO
            // ========================================================

            decimal? costoValido = null;

            if (!string.IsNullOrWhiteSpace(
                    TxtCostoEstimado.Text))
            {
                if (decimal.TryParse(
                        TxtCostoEstimado.Text,
                        out decimal parsedCosto))
                {
                    costoValido = parsedCosto;
                }
                else
                {
                    MessageBox.Show(
                        "El Costo Estimado debe ser un valor numérico válido.",
                        "Error de formato",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }
            }

            // ========================================================
            // 3. CREAMOS EL OBJETO
            // ========================================================

            Servicios servicioData = new Servicios
            {
                IdServicio = _idServicioActual,

                IdMoto = idMotoValido,

                IdEstado = idEstadoValido,

                FechaEntrada = _idServicioActual == 0
                    ? DateTime.Today
                    : DpFechaEntrada.SelectedDate
                        ?? DateTime.Today,

                CostoEstimado = costoValido,

                Detalle = TxtDetalle.Text
            };

            // ========================================================
            // 4. GUARDAMOS
            // ========================================================

            try
            {
                if (_idServicioActual == 0)
                {
                    // MODO CREACIÓN.
                    //
                    // SQL Server genera automáticamente
                    // FechaEntrada mediante DEFAULT.
                    //
                    _repo.AddServicio(servicioData);

                    MessageBox.Show(
                        "Servicio registrado con éxito.",
                        "Éxito",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    // MODO EDICIÓN.
                    //
                    // FechaEntrada original NO se modifica.
                    //
                    _repo.UpdateServicio(servicioData);

                    MessageBox.Show(
                        "Servicio actualizado con éxito.",
                        "Éxito",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }

                // Volvemos a la lista de servicios.
                NavegarAListaServicios();
            }
            catch (SqlException ex)
            {
                MessageBox.Show(
                    $"Error de base de datos: {ex.Message}",
                    "Error SQL",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ocurrió un error inesperado: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void NavegarAListaServicios()
        {
            MainWindow? mainWindow =
                Window.GetWindow(this) as MainWindow;

            if (mainWindow != null)
            {
                // Navegamos de vuelta al listado de servicios.
                mainWindow.ContentFrame.Navigate(
                    new ServiciosView());
            }
        }
    }
}
