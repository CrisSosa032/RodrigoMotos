using System;
using System.Windows;
using System.Windows.Controls;
using WpfNavigationProject.Models;
using WpfNavigationProject.Views;

namespace WpfNavigationProject
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Usuario que inició sesión
        private readonly Usuario _usuario;

        public MainWindow(Usuario usuario)
        {
            InitializeComponent();

            // Guardamos el usuario autenticado
            _usuario = usuario;

            // =========================================================
            // CONTROL DE PERMISOS
            // =========================================================

            // Si el usuario NO es administrador,
            // ocultamos el botón de Configuración.
            if (_usuario.Rol != "Admin")
            {
                BtnConfiguracion.Visibility = Visibility.Collapsed;
            }

            // =========================================================
            // VISTA INICIAL
            // =========================================================

            // Carga la vista de Inicio por defecto
            ContentFrame.Navigate(new Inicio(_usuario));
        }


        /// <summary>
        /// Maneja el evento Click de los botones del Sidebar.
        /// </summary>
        private void SidebarButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                // Obtenemos el Tag del botón
                string? viewTag = button.Tag?.ToString();

                if (string.IsNullOrEmpty(viewTag))
                {
                    return;
                }

                UserControl? newView = null;

                switch (viewTag)
                {
                    case "Home":
                        newView = new Inicio(_usuario);
                        break;

                    case "Clientes":
                        newView = new ClientesView();
                        break;

                    case "Motos":
                        newView = new MotosView();
                        break;

                    case "Servicios":
                        newView = new ServiciosView();
                        break;

                    case "Ganancias":
                        newView = new GananciasView();
                        break;

                    case "Configuracion":

                        // =====================================================
                        // SEGUNDA CAPA DE SEGURIDAD
                        // =====================================================
                        // Aunque el botón esté oculto para usuarios comunes,
                        // también comprobamos el rol antes de permitir
                        // la navegación.
                        if (_usuario.Rol != "Admin")
                        {
                            MessageBox.Show(
                                "No tienes permisos para acceder a esta sección.",
                                "Acceso denegado",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning
                            );

                            return;
                        }

                        
                        newView = new ConfiguracionView();

                        break;

                    default:
                        newView = new PlaceholderView(
                            "placeholder");
                        break;
                }

                if (newView != null)
                {
                    // Limpiamos la caché de navegación
                    ContentFrame.Content = null;

                    // Cargamos la nueva vista
                    ContentFrame.Navigate(newView);
                }
            }
        }
    }
}
