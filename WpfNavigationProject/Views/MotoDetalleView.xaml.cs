using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using WpfNavigationProject.DataAccess;
using WpfNavigationProject.Models;

namespace WpfNavigationProject.Views
{
    public partial class MotoDetalleView : UserControl
    {
        private readonly int _idMoto;

        private readonly MotoRepository _motoRepository =
            new MotoRepository();

        private readonly ServiciosRepository _serviciosRepository =
            new ServiciosRepository();

        public MotoDetalleView()
        {
            InitializeComponent();
        }

        public MotoDetalleView(int idMoto)
        {
            InitializeComponent();

            _idMoto = idMoto;

            CargarDatosMoto();
            CargarServicios();
        }

        // ============================================================
        // CARGAR DATOS DE LA MOTO Y DEL CLIENTE
        // ============================================================

        private void CargarDatosMoto()
        {
            try
            {
                MotoDetalle? moto =
                    _motoRepository.GetMotoDetalle(_idMoto);

                if (moto == null)
                {
                    MessageBox.Show(
                        "No se encontró la motocicleta seleccionada.",
                        "Moto no encontrada",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                TxtTituloMoto.Text =
                    $"{moto.Marca} {moto.Modelo}";

                TxtMarca.Text =
                    MostrarTexto(moto.Marca);

                TxtModelo.Text =
                    MostrarTexto(moto.Modelo);

                TxtAnio.Text =
                    moto.Anio.HasValue
                        ? moto.Anio.Value.ToString()
                        : "No especificado";

                TxtPatente.Text =
                    MostrarTexto(moto.Patente);

                TxtNroMotor.Text =
                    MostrarTexto(moto.NroMotor);

                TxtNroChasis.Text =
                    MostrarTexto(moto.NroChasis);

                TxtFechaAlta.Text =
                    moto.FechaAlta != DateTime.MinValue
                        ? moto.FechaAlta.ToString("dd/MM/yyyy")
                        : "No especificada";

                TxtObservaciones.Text =
                    string.IsNullOrWhiteSpace(moto.Observaciones)
                        ? "Sin observaciones"
                        : moto.Observaciones;

                TxtCliente.Text =
                    MostrarTexto(moto.NombreCliente);

                TxtDniCliente.Text =
                    MostrarTexto(moto.DNICliente);

                TxtTelefonoCliente.Text =
                    MostrarTexto(moto.TelefonoCliente);

                TxtDireccionCliente.Text =
                    MostrarTexto(moto.DireccionCliente);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ocurrió un error al cargar los datos de la motocicleta.\n\n"
                    + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // ============================================================
        // CARGAR SERVICIOS DE LA MOTO
        // ============================================================

        private void CargarServicios()
        {
            try
            {
                List<Servicios> servicios =
                    _serviciosRepository.GetServiciosByMoto(_idMoto);

                DgServicios.ItemsSource = servicios;

                int cantidad =
                    servicios.Count;

                TxtCantidadServicios.Text =
                    cantidad == 1
                        ? "1 servicio"
                        : $"{cantidad} servicios";

                if (cantidad == 0)
                {
                    TxtServicioSeleccionado.Text =
                        "Esta motocicleta todavía no tiene servicios registrados.";

                    PanelSinHistorial.Visibility =
                        Visibility.Visible;

                    DgHistorial.Visibility =
                        Visibility.Collapsed;
                }
                else
                {
                    TxtServicioSeleccionado.Text =
                        "Seleccioná un servicio para ver su historial.";

                    PanelSinHistorial.Visibility =
                        Visibility.Visible;

                    DgHistorial.Visibility =
                        Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ocurrió un error al cargar los servicios de la motocicleta.\n\n"
                    + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // ============================================================
        // SELECCIONAR SERVICIO
        // ============================================================

        private void DgServicios_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (DgServicios.SelectedItem is not Servicios servicio)
            {
                return;
            }

            CargarHistorialServicio(servicio);
        }


        // ============================================================
        // CARGAR HISTORIAL DEL SERVICIO
        // ============================================================

        private void CargarHistorialServicio(
            Servicios servicio)
        {
            try
            {
                List<ServicioHistorial> historial =
                    _serviciosRepository
                        .GetHistorialByServicio(
                            servicio.IdServicio);

                TxtServicioSeleccionado.Text =
                    $"Servicio del {servicio.FechaEntrada:dd/MM/yyyy}  •  " +
                    $"{servicio.NombreEstado}";

                if (!string.IsNullOrWhiteSpace(
                        servicio.Detalle))
                {
                    TxtServicioSeleccionado.Text +=
                        $"  •  {servicio.Detalle}";
                }

                DgHistorial.ItemsSource =
                    historial;

                if (historial.Count == 0)
                {
                    PanelSinHistorial.Visibility =
                        Visibility.Visible;

                    DgHistorial.Visibility =
                        Visibility.Collapsed;
                }
                else
                {
                    PanelSinHistorial.Visibility =
                        Visibility.Collapsed;

                    DgHistorial.Visibility =
                        Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ocurrió un error al cargar el historial del servicio.\n\n"
                    + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // ============================================================
        // MOSTRAR TEXTO
        // ============================================================

        private string MostrarTexto(string? texto)
        {
            return string.IsNullOrWhiteSpace(texto)
                ? "No especificado"
                : texto;
        }


        // ============================================================
        // VOLVER
        // ============================================================

        private void BtnVolver_Click(
            object sender,
            RoutedEventArgs e)
        {
            MainWindow? mainWindow =
                Window.GetWindow(this) as MainWindow;

            if (mainWindow != null)
            {
                MotosView motosView =
                    new MotosView();

                mainWindow.ContentFrame.Navigate(
                    motosView);
            }
        }
    }
}
