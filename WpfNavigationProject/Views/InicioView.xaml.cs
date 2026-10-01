using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using WpfNavigationProject.DataAccess;
using WpfNavigationProject.Models;

namespace WpfNavigationProject.Views
{
    public partial class Inicio : UserControl
    {
        private readonly ClienteRepository _clienteRepository;
        private readonly MotoRepository _motoRepository;
        private readonly ServiciosRepository _serviciosRepository;
        private readonly GananciasRepository _gananciasRepository;
        private readonly Usuario _usuario;

        // ============================================================
        // PAGINADO DE TRABAJOS EN CURSO
        // ============================================================

        private const int TrabajosPorPagina = 5;

        private int _paginaTrabajosActual = 1;
        private int _totalTrabajos = 0;
        private int _totalPaginasTrabajos = 1;

        public Inicio(Usuario usuario)
        {
            InitializeComponent();

            _usuario = usuario;

            _clienteRepository = new ClienteRepository();
            _motoRepository = new MotoRepository();
            _serviciosRepository = new ServiciosRepository();
            _gananciasRepository = new GananciasRepository();

            Loaded += Inicio_Loaded;
        }

        private void Inicio_Loaded(object sender, RoutedEventArgs e)
        {
            CargarInicio();
        }

        private void CargarInicio()
        {
            try
            {
                TxtSaludo.Text =
                    $"¡Hola, {_usuario.Nombre}! 👋";

                CargarResumenFacturacion();
                CargarResumenEstados();
                CargarGraficoEstados();
                CargarTrabajosEnCurso();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"No se pudo cargar el panel de control del taller.\n\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // ============================================================
        // RESUMEN DE FACTURACIÓN
        // ============================================================

        private void CargarResumenFacturacion()
        {
            DateTime hoy = DateTime.Today;

            DateTime fechaDesde =
                new DateTime(
                    hoy.Year,
                    hoy.Month,
                    1);

            DateTime fechaHasta = hoy;

            (decimal total, int cantidadServicios) =
                _gananciasRepository.GetResumenFacturacion(
                    fechaDesde,
                    fechaHasta);

            decimal ticketPromedio =
                cantidadServicios > 0
                    ? total / cantidadServicios
                    : 0m;

            CultureInfo cultura =
                new CultureInfo("es-AR");

            TxtFacturacionMes.Text =
                total.ToString("C", cultura);

            TxtServiciosCobrados.Text =
                cantidadServicios.ToString();

            TxtTicketPromedio.Text =
                ticketPromedio.ToString("C", cultura);

            string periodo =
                fechaDesde.ToString(
                    "MMMM yyyy",
                    cultura);

            TxtPeriodoFacturacion.Text =
                cultura.TextInfo.ToTitleCase(periodo);
        }

        // ============================================================
        // RESUMEN DE ESTADOS
        // ============================================================

        private void CargarResumenEstados()
        {
            int ingresadas =
                _serviciosRepository
                    .GetCantidadServiciosPorEstado(1);

            int enProceso =
                _serviciosRepository
                    .GetCantidadServiciosPorEstado(2);

            int terminadas =
                _serviciosRepository
                    .GetCantidadServiciosPorEstado(3);

            TxtResumenIngresadas.Text =
                ingresadas.ToString();

            TxtResumenEnProceso.Text =
                enProceso.ToString();

            TxtResumenTerminadas.Text =
                terminadas.ToString();
        }

        // ============================================================
        // GRÁFICO DE ESTADOS
        // ============================================================

        private void CargarGraficoEstados()
        {
            DateTime hoy = DateTime.Today;

            DateTime fechaDesde =
                new DateTime(
                    hoy.Year,
                    hoy.Month,
                    1);

            DateTime fechaHasta = hoy;

            TxtPeriodoGrafico.Text =
                $"Servicios ingresados desde {fechaDesde:dd/MM/yyyy} hasta {fechaHasta:dd/MM/yyyy}";

            Dictionary<int, int> resumen =
                _serviciosRepository
                    .GetResumenEstadosPorFecha(
                        fechaDesde,
                        fechaHasta);

            int ingresadas =
                ObtenerCantidadEstado(
                    resumen,
                    1);

            int enProceso =
                ObtenerCantidadEstado(
                    resumen,
                    2);

            int terminadas =
                ObtenerCantidadEstado(
                    resumen,
                    3);

            int total =
                ingresadas +
                enProceso +
                terminadas;

            TxtTotalServiciosMes.Text =
                total.ToString();

            TxtResumenTotalMes.Text =
                total.ToString();

            ChartEstados.Series =
                new ISeries[]
                {
                    new PieSeries<double>
                    {
                        Name = "Ingresadas",
                        Values = new double[]
                        {
                            ingresadas
                        }
                    },

                    new PieSeries<double>
                    {
                        Name = "En proceso",
                        Values = new double[]
                        {
                            enProceso
                        }
                    },

                    new PieSeries<double>
                    {
                        Name = "Terminadas",
                        Values = new double[]
                        {
                            terminadas
                        }
                    }
                };
        }

        private int ObtenerCantidadEstado(
            Dictionary<int, int> resumen,
            int idEstado)
        {
            if (resumen.TryGetValue(
                    idEstado,
                    out int cantidad))
            {
                return cantidad;
            }

            return 0;
        }

        // ============================================================
        // TRABAJOS EN CURSO
        // ============================================================

        private void CargarTrabajosEnCurso()
        {
            try
            {
                _totalTrabajos =
                    _serviciosRepository
                        .GetTotalTrabajosEnCurso();

                _totalPaginasTrabajos =
                    Math.Max(
                        1,
                        (int)Math.Ceiling(
                            (double)_totalTrabajos /
                            TrabajosPorPagina));

                if (_paginaTrabajosActual >
                    _totalPaginasTrabajos)
                {
                    _paginaTrabajosActual =
                        _totalPaginasTrabajos;
                }

                if (_paginaTrabajosActual < 1)
                {
                    _paginaTrabajosActual = 1;
                }

                List<TrabajoEnCurso> trabajos =
                    _serviciosRepository
                        .GetTrabajosEnCursoPaginados(
                            _paginaTrabajosActual,
                            TrabajosPorPagina);

                PanelTrabajos.Children.Clear();

                if (trabajos.Count == 0)
                {
                    MostrarSinTrabajos();
                }
                else
                {
                    foreach (TrabajoEnCurso trabajo in trabajos)
                    {
                        PanelTrabajos.Children.Add(
                            CrearTrabajoVisual(trabajo));
                    }
                }

                ActualizarPaginadoTrabajos();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"No se pudieron cargar los trabajos en curso.\n\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private Border CrearTrabajoVisual(
            TrabajoEnCurso trabajo)
        {
            Border contenedor = new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(18),
                Margin = new Thickness(0, 0, 0, 9),
                BorderBrush =
                    new SolidColorBrush(
                        Color.FromRgb(
                            229,
                            231,
                            235)),
                BorderThickness =
                    new Thickness(1)
            };

            Grid grid = new Grid();

            grid.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width =
                        new GridLength(
                            2.2,
                            GridUnitType.Star)
                });

            grid.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width =
                        new GridLength(
                            1.7,
                            GridUnitType.Star)
                });

            grid.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width =
                        new GridLength(
                            2.1,
                            GridUnitType.Star)
                });

            grid.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width =
                        GridLength.Auto
                });

            // ========================================================
            // MOTO
            // ========================================================

            StackPanel motoPanel =
                new StackPanel();

            TextBlock moto =
                new TextBlock
                {
                    Text =
                        $"🏍️  {trabajo.MotoDescripcion}",
                    FontSize = 14,
                    FontWeight =
                        FontWeights.SemiBold,
                    Foreground =
                        new SolidColorBrush(
                            Color.FromRgb(
                                31,
                                41,
                                55))
                };

            TextBlock patente =
                new TextBlock
                {
                    Text =
                        $"Patente: {trabajo.Patente}",
                    Margin =
                        new Thickness(
                            0,
                            4,
                            0,
                            0),
                    FontSize = 12,
                    Foreground =
                        new SolidColorBrush(
                            Color.FromRgb(
                                107,
                                114,
                                128))
                };

            motoPanel.Children.Add(moto);
            motoPanel.Children.Add(patente);

            Grid.SetColumn(
                motoPanel,
                0);

            grid.Children.Add(
                motoPanel);

            // ========================================================
            // CLIENTE
            // ========================================================

            StackPanel clientePanel =
                new StackPanel();

            TextBlock clienteTitulo =
                new TextBlock
                {
                    Text = "Cliente",
                    FontSize = 11,
                    Foreground =
                        new SolidColorBrush(
                            Color.FromRgb(
                                156,
                                163,
                                175))
                };

            TextBlock cliente =
                new TextBlock
                {
                    Text = trabajo.Cliente,
                    Margin =
                        new Thickness(
                            0,
                            3,
                            0,
                            0),
                    FontSize = 13,
                    Foreground =
                        new SolidColorBrush(
                            Color.FromRgb(
                                55,
                                65,
                                81))
                };

            clientePanel.Children.Add(
                clienteTitulo);

            clientePanel.Children.Add(
                cliente);

            Grid.SetColumn(
                clientePanel,
                1);

            grid.Children.Add(
                clientePanel);

            // ========================================================
            // SERVICIO
            // ========================================================

            StackPanel servicioPanel =
                new StackPanel();

            TextBlock servicioTitulo =
                new TextBlock
                {
                    Text = "Servicio",
                    FontSize = 11,
                    Foreground =
                        new SolidColorBrush(
                            Color.FromRgb(
                                156,
                                163,
                                175))
                };

            TextBlock servicio =
                new TextBlock
                {
                    Text =
                        string.IsNullOrWhiteSpace(
                            trabajo.Detalle)
                            ? "Sin detalle"
                            : trabajo.Detalle,

                    Margin =
                        new Thickness(
                            0,
                            3,
                            0,
                            0),

                    FontSize = 13,

                    Foreground =
                        new SolidColorBrush(
                            Color.FromRgb(
                                55,
                                65,
                                81)),

                    TextTrimming =
                        TextTrimming.CharacterEllipsis
                };

            servicioPanel.Children.Add(
                servicioTitulo);

            servicioPanel.Children.Add(
                servicio);

            Grid.SetColumn(
                servicioPanel,
                2);

            grid.Children.Add(
                servicioPanel);

            // ========================================================
            // ESTADO
            // ========================================================

            StackPanel estadoPanel =
            new StackPanel
            {
                HorizontalAlignment =
                    HorizontalAlignment.Right,

                VerticalAlignment =
                    VerticalAlignment.Center
            };

                Border estado =
                    new Border
                {
                    Background =
                        new SolidColorBrush(
                            Color.FromRgb(
                                255,
                                244,
                                214)),

                    CornerRadius =
                        new CornerRadius(8),

                    Padding =
                        new Thickness(
                            11,
                            6,
                            11,
                            6),

                    HorizontalAlignment =
                        HorizontalAlignment.Right,

                    Cursor =
                        Cursors.Hand,

                    ToolTip =
                        "Marcar servicio como terminado",

                    Tag =
                        trabajo.IdServicio
                };

            TextBlock estadoTexto =
                new TextBlock
                {
                    Text = "🔧 En curso",

                    FontSize = 11,

                    FontWeight =
                        FontWeights.SemiBold,

                    Foreground =
                        new SolidColorBrush(
                            Color.FromRgb(
                                154,
                                103,
                                0))
                };

            estado.Child =
                estadoTexto;


            // ========================================================
            // EFECTO HOVER
            // ========================================================

            estado.MouseEnter +=
                (sender, e) =>
                {
                    estado.Background =
                        new SolidColorBrush(
                            Color.FromRgb(
                                220,
                                252,
                                231));

                    estadoTexto.Text =
                        "✓ Marcar terminado";

                    estadoTexto.Foreground =
                        new SolidColorBrush(
                            Color.FromRgb(
                                22,
                                101,
                                52));
                };

            estado.MouseLeave +=
                (sender, e) =>
                {
                    estado.Background =
                        new SolidColorBrush(
                            Color.FromRgb(
                                255,
                                244,
                                214));

                    estadoTexto.Text =
                        "🔧 En curso";

                    estadoTexto.Foreground =
                        new SolidColorBrush(
                            Color.FromRgb(
                                154,
                                103,
                                0));
                };


            // ========================================================
            // CLICK
            // ========================================================

            estado.MouseLeftButtonUp +=
                (sender, e) =>
                {
                    MarcarTrabajoComoTerminado(
                        trabajo.IdServicio,
                        trabajo.MotoDescripcion);
                };

            estadoPanel.Children.Add(
                estado);

            string textoTiempo =
                ObtenerTextoTiempoTaller(
                    trabajo.FechaEntrada);

            TextBlock tiempo =
                new TextBlock
                {
                    Text = textoTiempo,

                    Margin =
                        new Thickness(
                            0,
                            5,
                            0,
                            0),

                    FontSize = 10,

                    Foreground =
                        new SolidColorBrush(
                            Color.FromRgb(
                                156,
                                163,
                                175)),

                    HorizontalAlignment =
                        HorizontalAlignment.Right
                };

            estadoPanel.Children.Add(
                tiempo);

            Grid.SetColumn(
                estadoPanel,
                3);

            grid.Children.Add(
                estadoPanel);

            contenedor.Child = grid;

            return contenedor;
        }

        private string ObtenerTextoTiempoTaller(
            DateTime fechaEntrada)
        {
            DateTime hoy =
                DateTime.Today;

            int dias =
                (hoy -
                    fechaEntrada.Date).Days;

            if (dias <= 0)
                return "Ingresó hoy";

            if (dias == 1)
                return "Hace 1 día";

            return $"Hace {dias} días";
        }

        // ============================================================
        // SIN TRABAJOS
        // ============================================================

        private void MostrarSinTrabajos()
        {
            Border contenedor =
                new Border
                {
                    Background =
                        Brushes.White,

                    CornerRadius =
                        new CornerRadius(10),

                    Padding =
                        new Thickness(35),

                    BorderBrush =
                        new SolidColorBrush(
                            Color.FromRgb(
                                229,
                                231,
                                235)),

                    BorderThickness =
                        new Thickness(1)
                };

            StackPanel panel =
                new StackPanel
                {
                    HorizontalAlignment =
                        HorizontalAlignment.Center
                };

            TextBlock icono =
                new TextBlock
                {
                    Text = "🔧",
                    FontSize = 38,
                    HorizontalAlignment =
                        HorizontalAlignment.Center
                };

            TextBlock titulo =
                new TextBlock
                {
                    Text =
                        "No hay trabajos en curso",

                    Margin =
                        new Thickness(
                            0,
                            12,
                            0,
                            0),

                    FontSize = 16,

                    FontWeight =
                        FontWeights.SemiBold,

                    Foreground =
                        new SolidColorBrush(
                            Color.FromRgb(
                                55,
                                65,
                                81)),

                    HorizontalAlignment =
                        HorizontalAlignment.Center
                };

            TextBlock descripcion =
                new TextBlock
                {
                    Text =
                        "Los servicios en proceso aparecerán aquí.",

                    Margin =
                        new Thickness(
                            0,
                            5,
                            0,
                            0),

                    FontSize = 13,

                    Foreground =
                        new SolidColorBrush(
                            Color.FromRgb(
                                156,
                                163,
                                175)),

                    HorizontalAlignment =
                        HorizontalAlignment.Center
                };

            panel.Children.Add(icono);
            panel.Children.Add(titulo);
            panel.Children.Add(descripcion);

            contenedor.Child = panel;

            PanelTrabajos.Children.Add(
                contenedor);
        }

        // ============================================================
        // PAGINADO DE TRABAJOS
        // ============================================================

        private void ActualizarPaginadoTrabajos()
        {
            TxtPaginaTrabajos.Text =
                $"Página {_paginaTrabajosActual} de {_totalPaginasTrabajos}";

            BtnPaginaTrabajosAnterior.IsEnabled =
                _paginaTrabajosActual > 1;

            BtnPaginaTrabajosSiguiente.IsEnabled =
                _paginaTrabajosActual <
                _totalPaginasTrabajos;
        }

        private void BtnPaginaTrabajosAnterior_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (_paginaTrabajosActual <= 1)
                return;

            _paginaTrabajosActual--;

            CargarTrabajosEnCurso();
        }

        private void BtnPaginaTrabajosSiguiente_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (_paginaTrabajosActual >=
                _totalPaginasTrabajos)
            {
                return;
            }

            _paginaTrabajosActual++;

            CargarTrabajosEnCurso();
        }

        // ============================================================
        // NAVEGACIÓN DE SERVICIOS
        // ============================================================

        private void ServiciosIngresados_Click(
            object sender,
            MouseButtonEventArgs e)
        {
            NavegarAServicios(1);
        }

        private void ServiciosEnProceso_Click(
            object sender,
            MouseButtonEventArgs e)
        {
            NavegarAServicios(2);
        }

        private void ServiciosTerminados_Click(
            object sender,
            MouseButtonEventArgs e)
        {
            NavegarAServicios(3);
        }

        private void NavegarAServicios(
            int idEstado)
        {
            MainWindow? mainWindow =
                Window.GetWindow(this)
                as MainWindow;

            if (mainWindow != null)
            {
                mainWindow.ContentFrame.Navigate(
                    new ServiciosView(idEstado));
            }
        }

        // ============================================================
        // ACCIONES RÁPIDAS
        // ============================================================

        private void NuevoCliente_Click(
            object sender,
            RoutedEventArgs e)
        {
            MainWindow? mainWindow =
                Window.GetWindow(this)
                as MainWindow;

            if (mainWindow != null)
            {
                mainWindow.ContentFrame.Navigate(
                    new ClienteFormView(0));
            }
        }

        private void NuevaMoto_Click(
            object sender,
            RoutedEventArgs e)
        {
            MainWindow? mainWindow =
                Window.GetWindow(this)
                as MainWindow;

            if (mainWindow != null)
            {
                mainWindow.ContentFrame.Navigate(
                    new MotoFormView(0));
            }
        }

        private void NuevoServicio_Click(
            object sender,
            RoutedEventArgs e)
        {
            MainWindow? mainWindow =
                Window.GetWindow(this)
                as MainWindow;

            if (mainWindow != null)
            {
                mainWindow.ContentFrame.Navigate(
                    new ServicioFormView(0));
            }
        }



        private void MarcarTrabajoComoTerminado(
            int idServicio,
            string descripcionMoto)
        {
            MessageBoxResult resultado =
                MessageBox.Show(
                    $"¿Deseás marcar como terminado el servicio de:\n\n" +
                    $"{descripcionMoto}\n\n" +
                    $"El servicio pasará de \"En proceso\" a \"Terminado\".",
                    "Confirmar servicio terminado",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

            if (resultado != MessageBoxResult.Yes)
                return;

            try
            {
                _serviciosRepository
                    .MarcarServicioTerminado(idServicio);

                MessageBox.Show(
                    "El servicio fue marcado como terminado correctamente.",
                    "Servicio actualizado",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // Actualizamos todo el dashboard
                CargarResumenEstados();
                CargarGraficoEstados();
                CargarTrabajosEnCurso();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"No se pudo actualizar el estado del servicio.\n\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}