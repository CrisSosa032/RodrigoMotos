using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;
using WpfNavigationProject.Models;
using WpfNavigationProject.DataAccess;

namespace WpfNavigationProject.Views
{
    public partial class UsuarioFormView : UserControl
    {
        // 1. Instanciamos el repositorio
        private UsuarioRepository _repo = new UsuarioRepository();


        // ID del usuario que estamos editando
        private int _idUsuarioActual;


        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        // Si idUsuario = 0 → Nuevo usuario
        // Si idUsuario > 0 → Editar usuario
        public UsuarioFormView(int idUsuario)
        {
            InitializeComponent();

            _idUsuarioActual = idUsuario;

            // Si el ID es mayor a 0, estamos editando
            if (_idUsuarioActual > 0)
            {
                PrellenarFormulario();

                TxtTitulo.Content = "Editar Usuario";
                BtnGuardar.Content = "Actualizar Usuario";
            }
            else
            {
                // Modo creación
                TxtTitulo.Content = "Registro de Usuarios";
                BtnGuardar.Content = "Guardar";

                // Rol por defecto
                if (CmbRol.Items.Count > 0)
                {
                    CmbRol.SelectedIndex = 0;
                }
            }
        }


        // =========================================================
        // CONSTRUCTOR PARA NUEVO USUARIO
        // =========================================================

        public UsuarioFormView() : this(0)
        {
        }


        // =========================================================
        // PRELLENAR FORMULARIO
        // =========================================================

        private void PrellenarFormulario()
        {
            try
            {
                // Buscamos nuevamente los datos en la base de datos
                Usuario? usuario = _repo.ObtenerUsuarioPorId(_idUsuarioActual);

                if (usuario != null)
                {
                    // Cargamos los datos del usuario
                    TxtUsername.Text = usuario.Username;
                    TxtNombre.Text = usuario.Nombre;

                    // Seleccionamos el rol actual
                    for (int i = 0; i < CmbRol.Items.Count; i++)
                    {
                        if (CmbRol.Items[i] is ComboBoxItem item &&
                            item.Content?.ToString() == usuario.Rol)
                        {
                            CmbRol.SelectedIndex = i;
                            break;
                        }
                    }

                    // La contraseña NO se carga.
                    // Al editar:
                    // - Vacía = conservar contraseña actual
                    // - Nueva contraseña = cambiar contraseña
                    TxtPassword.Clear();
                    TxtConfirmarPassword.Clear();
                }
                else
                {
                    MessageBox.Show(
                        "No se encontró el usuario solicitado.",
                        "Usuario no encontrado",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    NavegarAConfiguracion();
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


        // =========================================================
        // GUARDAR / ACTUALIZAR
        // =========================================================

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            // 1. Validaciones básicas

            if (string.IsNullOrWhiteSpace(TxtUsername.Text) ||
                string.IsNullOrWhiteSpace(TxtNombre.Text))
            {
                MessageBox.Show(
                    "El Usuario y el Nombre son obligatorios.",
                    "Validación",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (CmbRol.SelectedItem == null)
            {
                MessageBox.Show(
                    "Debe seleccionar un rol.",
                    "Validación",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }


            // 2. Obtenemos los datos actuales del formulario

            string username = TxtUsername.Text.Trim();
            string nombre = TxtNombre.Text.Trim();

            string rol =
                (CmbRol.SelectedItem as ComboBoxItem)?
                .Content?.ToString() ?? string.Empty;

            string password = TxtPassword.Password;
            string confirmarPassword = TxtConfirmarPassword.Password;


            // 3. Validación de contraseña

            if (_idUsuarioActual == 0)
            {
                // Al crear un usuario la contraseña es obligatoria

                if (string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show(
                        "La contraseña es obligatoria para crear un usuario.",
                        "Validación",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    TxtPassword.Focus();
                    return;
                }
            }

            // Si alguno de los campos tiene contenido,
            // ambos deben coincidir.

            if (!string.IsNullOrWhiteSpace(password) ||
                !string.IsNullOrWhiteSpace(confirmarPassword))
            {
                if (password != confirmarPassword)
                {
                    MessageBox.Show(
                        "Las contraseñas no coinciden.",
                        "Validación",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    TxtConfirmarPassword.Clear();
                    TxtConfirmarPassword.Focus();

                    return;
                }
            }


            // 4. Creamos el objeto con los datos actuales

            Usuario usuarioData = new Usuario
            {
                IdUsuario = _idUsuarioActual,
                Username = username,
                Nombre = nombre,
                Rol = rol
            };


            try
            {
                if (_idUsuarioActual == 0)
                {
                    // =================================================
                    // MODO CREACIÓN
                    // =================================================

                    _repo.RegistrarUsuario(
                        usuarioData.Username,
                        password,
                        usuarioData.Nombre,
                        usuarioData.Rol);

                    MessageBox.Show(
                        "Usuario registrado con éxito.",
                        "Éxito",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    // =================================================
                    // MODO EDICIÓN
                    // =================================================

                    bool actualizado = _repo.ActualizarUsuario(
                        usuarioData.IdUsuario,
                        usuarioData.Username,
                        usuarioData.Nombre,
                        usuarioData.Rol,
                        password);

                    if (!actualizado)
                    {
                        MessageBox.Show(
                            "No se pudo actualizar el usuario.",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);

                        return;
                    }

                    MessageBox.Show(
                        "Usuario actualizado con éxito.",
                        "Éxito",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }


                // 5. Volvemos a la lista de usuarios

                NavegarAConfiguracion();
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
                    $"Ocurrió un error: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // CANCELAR
        // =========================================================

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            NavegarAConfiguracion();
        }


        // =========================================================
        // NAVEGAR A CONFIGURACIÓN
        // =========================================================

        private void NavegarAConfiguracion()
        {
            MainWindow? mainWindow =
                Window.GetWindow(this) as MainWindow;

            if (mainWindow != null)
            {
                mainWindow.ContentFrame.Navigate(
                    new ConfiguracionView());
            }
        }
    }


}
