using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using WpfNavigationProject.DataAccess;
using WpfNavigationProject.Models;

namespace WPF_LoginForm.View
{
    public partial class RecuperarPasswordView : Window
    {
        private Usuario? usuarioEncontrado;

        private string codigoRecuperacion = string.Empty;

        private DateTime fechaExpiracion;

        private int intentos = 0;

        private const int MAX_INTENTOS = 3;

        private DispatcherTimer? timer;


        public RecuperarPasswordView()
        {
            InitializeComponent();
        }


        // =========================================================
        // MOVER VENTANA
        // =========================================================

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }


        // =========================================================
        // CERRAR
        // =========================================================

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            CerrarTemporizador();

            Close();
        }


        // =========================================================
        // PASO 1
        // BUSCAR USUARIO
        // =========================================================

        private void btnEnviarCodigo_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsuario.Text.Trim();

            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show(
                    "Ingresá tu usuario.",
                    "Recuperar contraseña",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                txtUsuario.Focus();

                return;
            }


            UsuarioRepository repo = new UsuarioRepository();

            usuarioEncontrado =
                repo.ObtenerUsuarioPorUsername(username);


            if (usuarioEncontrado == null)
            {
                MessageBox.Show(
                    "No se encontró ningún usuario con ese nombre.",
                    "Recuperar contraseña",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }


            // =====================================================
            // GENERAR CÓDIGO
            // =====================================================

            Random random = new Random();

            codigoRecuperacion =
                random.Next(100000, 1000000).ToString();


            // El código tendrá una duración de 5 minutos

            fechaExpiracion =
                DateTime.Now.AddMinutes(5);


            intentos = 0;


            // =====================================================
            // SIMULACIÓN DEL ENVÍO DEL CORREO
            // =====================================================

            MessageBox.Show(
                $"SIMULACIÓN DE ENVÍO DE CORREO\n\n" +
                $"Usuario: {usuarioEncontrado.Username}\n\n" +
                $"Código de recuperación:\n\n" +
                $"{codigoRecuperacion}\n\n" +
                $"Este código será válido durante 5 minutos.",

                "Correo simulado",

                MessageBoxButton.OK,
                MessageBoxImage.Information
            );


            // Pasamos al paso 2

            panelUsuario.Visibility =
                Visibility.Collapsed;

            panelCodigo.Visibility =
                Visibility.Visible;

            panelNuevaPassword.Visibility =
                Visibility.Collapsed;


            txtCodigo.Clear();

            txtCodigo.Focus();


            // Iniciamos temporizador

            IniciarTemporizador();
        }


        // =========================================================
        // PASO 2
        // VERIFICAR CÓDIGO
        // =========================================================

        private void btnVerificarCodigo_Click(
            object sender,
            RoutedEventArgs e)
        {
            string codigoIngresado =
                txtCodigo.Text.Trim();


            if (string.IsNullOrWhiteSpace(codigoIngresado))
            {
                MessageBox.Show(
                    "Ingresá el código de recuperación.",
                    "Verificación",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                txtCodigo.Focus();

                return;
            }


            // =====================================================
            // COMPROBAR EXPIRACIÓN
            // =====================================================

            if (DateTime.Now > fechaExpiracion)
            {
                CerrarTemporizador();

                MessageBox.Show(
                    "El código de recuperación ha expirado.\n\n" +
                    "Solicitá un nuevo código.",
                    "Código expirado",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                VolverAlInicio();

                return;
            }


            // =====================================================
            // COMPROBAR INTENTOS
            // =====================================================

            if (intentos >= MAX_INTENTOS)
            {
                CerrarTemporizador();

                MessageBox.Show(
                    "Has superado el número máximo de intentos.\n\n" +
                    "Solicitá un nuevo código.",
                    "Demasiados intentos",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                VolverAlInicio();

                return;
            }


            // =====================================================
            // COMPARAR CÓDIGO
            // =====================================================

            if (codigoIngresado != codigoRecuperacion)
            {
                intentos++;

                int restantes =
                    MAX_INTENTOS - intentos;


                if (restantes > 0)
                {
                    MessageBox.Show(
                        $"El código ingresado es incorrecto.\n\n" +
                        $"Intentos restantes: {restantes}",

                        "Código incorrecto",

                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );

                    txtCodigo.SelectAll();
                    txtCodigo.Focus();

                    return;
                }


                // Se acabaron los intentos

                CerrarTemporizador();

                MessageBox.Show(
                    "Has superado el número máximo de intentos.\n\n" +
                    "Solicitá un nuevo código.",

                    "Verificación fallida",

                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                VolverAlInicio();

                return;
            }


            // =====================================================
            // CÓDIGO CORRECTO
            // =====================================================

            CerrarTemporizador();


            panelUsuario.Visibility =
                Visibility.Collapsed;

            panelCodigo.Visibility =
                Visibility.Collapsed;

            panelNuevaPassword.Visibility =
                Visibility.Visible;


            txtNuevaPassword.Clear();
            txtConfirmarPassword.Clear();

            txtNuevaPassword.Focus();
        }


        // =========================================================
        // PASO 3
        // CAMBIAR CONTRASEÑA
        // =========================================================

        private void btnCambiarPassword_Click(
            object sender,
            RoutedEventArgs e)
        {
            string nuevaPassword =
                txtNuevaPassword.Password;

            string confirmarPassword =
                txtConfirmarPassword.Password;


            // =====================================================
            // VALIDACIONES
            // =====================================================

            if (string.IsNullOrWhiteSpace(nuevaPassword))
            {
                MessageBox.Show(
                    "Ingresá una nueva contraseña.",
                    "Nueva contraseña",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                txtNuevaPassword.Focus();

                return;
            }


            if (nuevaPassword.Length < 6)
            {
                MessageBox.Show(
                    "La contraseña debe tener al menos 6 caracteres.",
                    "Nueva contraseña",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                txtNuevaPassword.Focus();

                return;
            }


            if (string.IsNullOrWhiteSpace(confirmarPassword))
            {
                MessageBox.Show(
                    "Confirmá la nueva contraseña.",
                    "Nueva contraseña",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                txtConfirmarPassword.Focus();

                return;
            }


            if (nuevaPassword != confirmarPassword)
            {
                MessageBox.Show(
                    "Las contraseñas no coinciden.",
                    "Nueva contraseña",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                txtConfirmarPassword.SelectAll();
                txtConfirmarPassword.Focus();

                return;
            }


            if (usuarioEncontrado == null)
            {
                MessageBox.Show(
                    "No se pudo identificar el usuario.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );

                return;
            }


            // =====================================================
            // ACTUALIZAR CONTRASEÑA
            // =====================================================

            UsuarioRepository repo =
                new UsuarioRepository();


            bool actualizada =
                repo.ActualizarPassword(
                    usuarioEncontrado.IdUsuario,
                    nuevaPassword
                );


            if (actualizada)
            {
                MessageBox.Show(
                    "La contraseña fue cambiada correctamente.\n\n" +
                    "Ahora podés iniciar sesión con tu nueva contraseña.",

                    "Recuperación exitosa",

                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );


                // El código deja de ser válido

                codigoRecuperacion = string.Empty;

                CerrarTemporizador();

                Close();
            }
            else
            {
                MessageBox.Show(
                    "No se pudo actualizar la contraseña.\n\n" +
                    "Intentá nuevamente.",

                    "Error",

                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }


        // =========================================================
        // TEMPORIZADOR
        // =========================================================

        private void IniciarTemporizador()
        {
            CerrarTemporizador();


            timer = new DispatcherTimer();

            timer.Interval =
                TimeSpan.FromSeconds(1);

            timer.Tick += Timer_Tick;

            timer.Start();


            ActualizarTiempo();
        }


        private void Timer_Tick(
            object? sender,
            EventArgs e)
        {
            if (DateTime.Now >= fechaExpiracion)
            {
                CerrarTemporizador();

                lblTiempo.Text =
                    "El código ha expirado.";

                btnVerificarCodigo.IsEnabled =
                    false;

                return;
            }


            ActualizarTiempo();
        }


        private void ActualizarTiempo()
        {
            TimeSpan restante =
                fechaExpiracion - DateTime.Now;


            if (restante.TotalSeconds <= 0)
            {
                lblTiempo.Text =
                    "El código ha expirado.";

                return;
            }


            lblTiempo.Text =
                $"El código vence en " +
                $"{restante.Minutes:00}:{restante.Seconds:00}";
        }


        private void CerrarTemporizador()
        {
            if (timer != null)
            {
                timer.Stop();

                timer.Tick -= Timer_Tick;

                timer = null;
            }
        }


        // =========================================================
        // VOLVER AL PASO 1
        // =========================================================

        private void VolverAlInicio()
        {
            CerrarTemporizador();

            codigoRecuperacion =
                string.Empty;

            usuarioEncontrado =
                null;

            intentos = 0;


            panelUsuario.Visibility =
                Visibility.Visible;

            panelCodigo.Visibility =
                Visibility.Collapsed;

            panelNuevaPassword.Visibility =
                Visibility.Collapsed;


            txtCodigo.Clear();

            txtUsuario.Focus();


            btnVerificarCodigo.IsEnabled =
                true;
        }


        // =========================================================
        // CERRAR VENTANA
        // =========================================================

        protected override void OnClosed(EventArgs e)
        {
            CerrarTemporizador();

            base.OnClosed(e);
        }
    }
}