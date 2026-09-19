using System;
using System.Windows;
using System.Windows.Controls;
using WpfNavigationProject.DataAccess;
using WpfNavigationProject.Models;

namespace WpfNavigationProject.Views
{
    public partial class ConfiguracionView : UserControl
    {
        private readonly UsuarioRepository _usuarioRepository;


    // =========================================================
    // CONSTRUCTOR
    // =========================================================
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
            catch (Exception ex)
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


        // =========================================================
        // EDITAR USUARIO
        // =========================================================
        private void BtnEditarUsuario_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                // -------------------------------------------------
                // Obtenemos el usuario correspondiente a la fila
                // donde se presionó el botón Editar.
                // -------------------------------------------------

                if (sender is Button boton &&
                    boton.DataContext is Usuario usuario)
                {
                    MainWindow? mainWindow =
                        Window.GetWindow(this) as MainWindow;

                    if (mainWindow != null)
                    {
                        // -------------------------------------------------
                        // IMPORTANTE:
                        // Enviamos el ID del usuario al formulario.
                        // El formulario consultará nuevamente la base
                        // de datos y precargará sus datos.
                        // -------------------------------------------------

                        mainWindow.ContentFrame.Navigate(
                            new UsuarioFormView(usuario.IdUsuario));
                    }
                }
                else
                {
                    MessageBox.Show(
                        "No se pudo obtener el usuario seleccionado.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo abrir el formulario de edición.\n\n" +
                    "Error: " + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }


        // =========================================================
        // NUEVO USUARIO
        // =========================================================
        private void BtnNuevoUsuario_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                MainWindow? mainWindow =
                    Window.GetWindow(this) as MainWindow;

                if (mainWindow != null)
                {
                    // Constructor sin parámetros = Nuevo usuario
                    mainWindow.ContentFrame.Navigate(
                        new UsuarioFormView());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo abrir el formulario de nuevo usuario.\n\n" +
                    "Error: " + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
    }


}
