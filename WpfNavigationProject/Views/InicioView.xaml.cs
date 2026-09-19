using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using WpfNavigationProject.DataAccess;
using WpfNavigationProject.Models;


namespace WpfNavigationProject.Views
{
    public partial class Inicio : UserControl
    {
        private readonly ClienteRepository _clienteRepository;
        private readonly MotoRepository _motoRepository;
        private readonly ServiciosRepository _serviciosRepository;
        private readonly Usuario _usuario;

        public Inicio(Usuario usuario)
        {
            InitializeComponent();

            _usuario = usuario;

            _clienteRepository = new ClienteRepository();
            _motoRepository = new MotoRepository();
            _serviciosRepository = new ServiciosRepository();

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
                TxtSaludo.Text = $"¡Hola, {_usuario.Nombre}! 👋";

                CargarResumen();
                CargarTrabajosEnCurso();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"No se pudo cargar el resumen del taller.\n\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void CargarResumen()
        {
            List<Cliente> clientes = _clienteRepository.GetAllClientes();
            List<Moto> motos = _motoRepository.GetAllMotos();
            List<TrabajoEnCurso> trabajos =
                _serviciosRepository.GetTrabajosEnCurso();

            TxtCantidadClientes.Text = clientes.Count.ToString();
            TxtCantidadMotos.Text = motos.Count.ToString();
            TxtCantidadTrabajos.Text = trabajos.Count.ToString();
        }

        private void CargarTrabajosEnCurso()
        {
            List<TrabajoEnCurso> trabajos =
                _serviciosRepository.GetTrabajosEnCurso();

            PanelTrabajos.Children.Clear();

            if (trabajos.Count == 0)
            {
                MostrarSinTrabajos();
                return;
            }

            foreach (TrabajoEnCurso trabajo in trabajos)
            {
                PanelTrabajos.Children.Add(
                    CrearTrabajoVisual(trabajo));
            }
        }

        // =========================================================
        // ACCIONES RÁPIDAS
        // =========================================================

        /// <summary>
        /// Abre el formulario para registrar un nuevo cliente.
        /// El ID 0 indica que estamos en modo creación.
        /// </summary>
        private void NuevoCliente_Click(object sender, RoutedEventArgs e)
        {
            MainWindow? mainWindow = Window.GetWindow(this) as MainWindow;

            if (mainWindow != null)
            {
                mainWindow.ContentFrame.Navigate(
                    new ClienteFormView(0));
            }
        }

        /// <summary>
        /// Abre el formulario para registrar una nueva moto.
        /// El ID 0 indica que estamos en modo creación.
        /// </summary>
        private void NuevaMoto_Click(object sender, RoutedEventArgs e)
        {
            MainWindow? mainWindow = Window.GetWindow(this) as MainWindow;

            if (mainWindow != null)
            {
                mainWindow.ContentFrame.Navigate(
                    new MotoFormView(0));
            }
        }

        /// <summary>
        /// Abre el formulario para registrar un nuevo servicio.
        /// El ID 0 indica que estamos en modo creación.
        /// </summary>
        private void NuevoServicio_Click(object sender, RoutedEventArgs e)
        {
            MainWindow? mainWindow = Window.GetWindow(this) as MainWindow;

            if (mainWindow != null)
            {
                mainWindow.ContentFrame.Navigate(
                    new ServicioFormView(0));
            }
        }

        // =========================================================
        // TRABAJOS EN CURSO
        // =========================================================

        private Border CrearTrabajoVisual(TrabajoEnCurso trabajo)
        {
            Border contenedor = new Border
            {
                Background = System.Windows.Media.Brushes.White,
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(20),
                Margin = new Thickness(0, 0, 0, 10),
                BorderBrush = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(229, 231, 235)),
                BorderThickness = new Thickness(1)
            };

            Grid grid = new Grid();

            grid.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width = new GridLength(2, GridUnitType.Star)
                });

            grid.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width = new GridLength(2, GridUnitType.Star)
                });

            grid.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width = new GridLength(2, GridUnitType.Star)
                });

            grid.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width = GridLength.Auto
                });

            // ==========================
            // MOTO
            // ==========================

            StackPanel motoPanel = new StackPanel();

            TextBlock moto = new TextBlock
            {
                Text = $"🏍️  {trabajo.MotoDescripcion}",
                FontSize = 15,
                FontWeight = FontWeights.SemiBold,
                Foreground = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(31, 41, 55))
            };

            TextBlock patente = new TextBlock
            {
                Text = $"Patente: {trabajo.Patente}",
                Margin = new Thickness(0, 5, 0, 0),
                FontSize = 12,
                Foreground = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(107, 114, 128))
            };

            motoPanel.Children.Add(moto);
            motoPanel.Children.Add(patente);

            Grid.SetColumn(motoPanel, 0);
            grid.Children.Add(motoPanel);

            // ==========================
            // CLIENTE
            // ==========================

            StackPanel clientePanel = new StackPanel();

            TextBlock clienteTitulo = new TextBlock
            {
                Text = "Cliente",
                FontSize = 11,
                Foreground = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(156, 163, 175))
            };

            TextBlock cliente = new TextBlock
            {
                Text = trabajo.Cliente,
                Margin = new Thickness(0, 4, 0, 0),
                FontSize = 14,
                Foreground = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(55, 65, 81))
            };

            clientePanel.Children.Add(clienteTitulo);
            clientePanel.Children.Add(cliente);

            Grid.SetColumn(clientePanel, 1);
            grid.Children.Add(clientePanel);

            // ==========================
            // SERVICIO
            // ==========================

            StackPanel servicioPanel = new StackPanel();

            TextBlock servicioTitulo = new TextBlock
            {
                Text = "Servicio",
                FontSize = 11,
                Foreground = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(156, 163, 175))
            };

            TextBlock servicio = new TextBlock
            {
                Text = string.IsNullOrWhiteSpace(trabajo.Detalle)
                    ? "Sin detalle"
                    : trabajo.Detalle,
                Margin = new Thickness(0, 4, 0, 0),
                FontSize = 14,
                Foreground = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(55, 65, 81))
            };

            servicioPanel.Children.Add(servicioTitulo);
            servicioPanel.Children.Add(servicio);

            Grid.SetColumn(servicioPanel, 2);
            grid.Children.Add(servicioPanel);

            // ==========================
            // ESTADO
            // ==========================

            Border estado = new Border
            {
                Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(255, 244, 214)),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12, 7, 12, 7),
                VerticalAlignment = VerticalAlignment.Center
            };

            TextBlock estadoTexto = new TextBlock
            {
                Text = "🔧 En curso",
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Foreground = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(154, 103, 0))
            };

            estado.Child = estadoTexto;

            Grid.SetColumn(estado, 3);
            grid.Children.Add(estado);

            contenedor.Child = grid;

            return contenedor;
        }

        private void MostrarSinTrabajos()
        {
            Border contenedor = new Border
            {
                Background = System.Windows.Media.Brushes.White,
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(40),
                BorderBrush = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(229, 231, 235)),
                BorderThickness = new Thickness(1)
            };

            StackPanel panel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center
            };

            TextBlock icono = new TextBlock
            {
                Text = "🔧",
                FontSize = 40,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            TextBlock titulo = new TextBlock
            {
                Text = "No hay trabajos en curso",
                Margin = new Thickness(0, 15, 0, 0),
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Foreground = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(55, 65, 81)),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            TextBlock descripcion = new TextBlock
            {
                Text = "Los servicios activos aparecerán aquí.",
                Margin = new Thickness(0, 5, 0, 0),
                FontSize = 13,
                Foreground = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(156, 163, 175)),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            panel.Children.Add(icono);
            panel.Children.Add(titulo);
            panel.Children.Add(descripcion);

            contenedor.Child = panel;

            PanelTrabajos.Children.Add(contenedor);
        }
    }

}