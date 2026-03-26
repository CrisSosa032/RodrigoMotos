using System.Windows;
using System.Windows.Controls;
using WpfNavigationProject.Views;
using WpfNavigationProject.DataAccess;

namespace WpfNavigationProject
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();


            // Carga la vista de 'Inicio' por defecto al iniciar la aplicación.
            // Asegúrate de que 'HomeView' exista en la carpeta 'Views'.
            ContentFrame.Navigate(new HomeView());
        }

        /// <summary>
        /// Maneja el evento Click de los botones del Sidebar.
        /// </summary>
        private void SidebarButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                // Obtenemos el Tag del botón para determinar qué vista cargar.
                string viewTag = button.Tag?.ToString();

                if (string.IsNullOrEmpty(viewTag))
                {
                    return; // No hacer nada si no hay Tag
                }

                // Dependiendo del Tag, navegamos a la vista correspondiente.
                // En WPF, lo más común es navegar a un nuevo UserControl (página).
                UserControl newView = null;

                switch (viewTag)
                {
                    case "Home":
                        newView = new HomeView();
                        break;
                    case "Clientes":
                        newView = new ClientesView();
                        break;
                    case "Motos":
                        // Aquí navegaremos a la nueva vista cuando la creemos
                        // ContentFrame.Navigate(new MotosView()); 
                        MessageBox.Show("Cargando sección de Motos...");
                        break;
                    case "Analiticas":
                        // newView = new AnaliticasView(); // Necesitas crear este archivo
                        newView = new PlaceholderView("Sección de Analíticas");
                        break;
                    case "Configuracion":
                        // newView = new ConfiguracionView(); // Necesitas crear este archivo
                        newView = new PlaceholderView("Sección de Configuración");
                        break;
                    default:
                        // Vista por defecto o mensaje de error
                        newView = new PlaceholderView("Página no encontrada");
                        break;
                }

                if (newView != null)
                {
                    // Limpiamos la caché de navegación antes de cargar una nueva página
                    ContentFrame.Content = null;
                    ContentFrame.Navigate(newView);
                }
            }
        }
    }
}