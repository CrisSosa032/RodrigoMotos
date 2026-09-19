using System.Windows;
using System.Windows.Controls;
using WpfNavigationProject.DataAccess;

namespace WpfNavigationProject.Views
{
    public partial class ConfiguracionView : UserControl
    {
        private readonly UsuarioRepository _usuarioRepository;

        public ConfiguracionView()
        {
            InitializeComponent();

            _usuarioRepository = new UsuarioRepository();

            CargarUsuarios();
        }


        // =========================================================
        // CARGAR USUARIOS
        // =========================================================
        private void CargarUsuarios()
        {
            try
            {
                var usuarios = _usuarioRepository.ObtenerTodosLosUsuarios();

                UsuariosDataGrid.ItemsSource = usuarios;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(
                    "No se pudieron cargar los usuarios.\n\n" +
                    "Error: " + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
    }
}
