using System.Windows;
using System.Windows.Input;
using WpfNavigationProject;
using WpfNavigationProject.DataAccess;
using WpfNavigationProject.Models;

namespace WPF_LoginForm.View
{
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            UsuarioRepository repo = new UsuarioRepository();

            var usuario = repo.ValidarUsuario(
                txtUser.Text,
                txtPass.Password
            );

            if (usuario != null)
            {
                // =====================================================
                // DESACTIVACIÓN AUTOMÁTICA DE CLIENTES INACTIVOS
                // =====================================================

                ClienteRepository clienteRepository =
                    new ClienteRepository();

                clienteRepository.DesactivarClientesInactivos();


                // =====================================================
                // ABRIR APLICACIÓN
                // =====================================================

                // Pasamos el usuario autenticado al MainWindow
                MainWindow main = new MainWindow(usuario);

                main.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show(
                    "Usuario o contraseña incorrectos.",
                    "Error de inicio de sesión",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
        }


        // =========================================================
        // RECUPERAR CONTRASEÑA
        // =========================================================

        private void txtReiniciar_MouseLeftButtonDown(
            object sender,
            MouseButtonEventArgs e)
        {
            RecuperarPasswordView ventana =
                new RecuperarPasswordView();

            ventana.Owner = this;

            ventana.ShowDialog();
        }
    }
}
