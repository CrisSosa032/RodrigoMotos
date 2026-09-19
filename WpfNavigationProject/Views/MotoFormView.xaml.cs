using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WpfNavigationProject.DataAccess;
using WpfNavigationProject.Models;

namespace WpfNavigationProject.Views
{
    public partial class MotoFormView : UserControl
    {
        private readonly MotoRepository _repo;
        private readonly ClienteRepository _clienteRepository;

        private readonly int _idMotoActual;

        private List<Cliente> _clientes = new List<Cliente>();

        private int _idClienteSeleccionado = 0;


        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public MotoFormView(int idMoto)
        {
            InitializeComponent();

            _repo = new MotoRepository();
            _clienteRepository = new ClienteRepository();

            _idMotoActual = idMoto;

            CargarClientes();

            if (_idMotoActual > 0)
            {
                CargarMoto();
            }
        }


        // =========================================================
        // CARGAR CLIENTES ACTIVOS
        // =========================================================

        private void CargarClientes()
        {
            try
            {
                _clientes = _clienteRepository.GetAllClientes();

                LstClientes.ItemsSource = null;

                LstClientes.Visibility =
                    Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudieron cargar los clientes.\n\n"
                    + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // BUSCAR CLIENTE
        // =========================================================

        private void TxtBuscarCliente_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            if (_clientes == null)
                return;

            string texto =
                TxtBuscarCliente.Text.Trim();

            if (string.IsNullOrWhiteSpace(texto))
            {
                LstClientes.ItemsSource = null;

                LstClientes.Visibility =
                    Visibility.Collapsed;

                return;
            }


            List<Cliente> resultados =
                _clientes
                    .Where(c =>
                        (!string.IsNullOrWhiteSpace(c.Nombre)
                         &&
                         c.Nombre.IndexOf(
                             texto,
                             StringComparison.OrdinalIgnoreCase) >= 0)
                        ||
                        (!string.IsNullOrWhiteSpace(c.DNI)
                         &&
                         c.DNI.IndexOf(
                             texto,
                             StringComparison.OrdinalIgnoreCase) >= 0))
                    .OrderBy(c => c.Nombre)
                    .ToList();


            LstClientes.ItemsSource =
                resultados;


            LstClientes.Visibility =
                resultados.Count > 0
                    ? Visibility.Visible
                    : Visibility.Collapsed;
        }


        // =========================================================
        // SELECCIONAR CLIENTE
        // =========================================================

        private void LstClientes_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (LstClientes.SelectedItem is not Cliente cliente)
                return;


            _idClienteSeleccionado =
                cliente.IdCliente;


            TxtClienteSeleccionado.Text =
                $"{cliente.Nombre}   |   DNI: {cliente.DNI}";


            PanelClienteSeleccionado.Visibility =
                Visibility.Visible;


            TxtBuscarCliente.Text =
                string.Empty;


            LstClientes.ItemsSource =
                null;


            LstClientes.Visibility =
                Visibility.Collapsed;


            LstClientes.SelectedItem =
                null;
        }


        // =========================================================
        // CAMBIAR CLIENTE
        // =========================================================

        private void BtnCambiarCliente_Click(
            object sender,
            RoutedEventArgs e)
        {
            _idClienteSeleccionado = 0;


            TxtClienteSeleccionado.Text =
                string.Empty;


            PanelClienteSeleccionado.Visibility =
                Visibility.Collapsed;


            TxtBuscarCliente.Focus();
        }


        // =========================================================
        // CARGAR MOTO PARA EDICIÓN
        // =========================================================

        private void CargarMoto()
        {
            try
            {
                Moto ?moto =
                    _repo.GetMotoById(_idMotoActual);


                if (moto == null)
                {
                    MessageBox.Show(
                        "No se encontró la moto seleccionada.",
                        "Aviso",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }


                // -------------------------------------------------
                // TÍTULO
                // -------------------------------------------------

                TxtTitulo.Text =
                    "Editar moto";

                BtnCargarMoto.Content =
                    "Guardar cambios";


                // -------------------------------------------------
                // CLIENTE
                // -------------------------------------------------

                _idClienteSeleccionado =
                    moto.IdCliente;


                Cliente? cliente =
                    _clienteRepository.GetClienteById(
                        moto.IdCliente);


                if (cliente != null)
                {
                    TxtClienteSeleccionado.Text =
                        $"{cliente.Nombre}   |   DNI: {cliente.DNI}";

                    PanelClienteSeleccionado.Visibility =
                        Visibility.Visible;
                }


                // -------------------------------------------------
                // DATOS DE LA MOTO
                // -------------------------------------------------

                TxtMarca.Text =
                    moto.Marca ?? string.Empty;

                TxtModelo.Text =
                    moto.Modelo ?? string.Empty;

                TxtAnio.Text = moto.Anio.HasValue
                    ? moto.Anio.Value.ToString()
                    : string.Empty;

                TxtPatente.Text =
                    moto.Patente ?? string.Empty;

                TxtMotor.Text =
                    moto.NroMotor ?? string.Empty;

                TxtChasis.Text =
                    moto.NroChasis ?? string.Empty;

                TxtObservaciones.Text =
                    moto.Observaciones ?? string.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo cargar la moto.\n\n"
                    + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // GUARDAR / ACTUALIZAR
        // =========================================================

        private void BtnCargarMoto_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                // -------------------------------------------------
                // VALIDAR CLIENTE
                // -------------------------------------------------

                if (_idClienteSeleccionado <= 0)
                {
                    MessageBox.Show(
                        "Primero debés seleccionar un cliente.",
                        "Cliente requerido",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    TxtBuscarCliente.Focus();

                    return;
                }


                // -------------------------------------------------
                // VALIDAR MARCA
                // -------------------------------------------------

                if (string.IsNullOrWhiteSpace(
                    TxtMarca.Text))
                {
                    MessageBox.Show(
                        "Ingresá la marca de la moto.",
                        "Dato requerido",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    TxtMarca.Focus();

                    return;
                }


                // -------------------------------------------------
                // VALIDAR MODELO
                // -------------------------------------------------

                if (string.IsNullOrWhiteSpace(
                    TxtModelo.Text))
                {
                    MessageBox.Show(
                        "Ingresá el modelo de la moto.",
                        "Dato requerido",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    TxtModelo.Focus();

                    return;
                }


                // -------------------------------------------------
                // AÑO
                // -------------------------------------------------

                short? anio = null;

                if (!string.IsNullOrWhiteSpace(TxtAnio.Text))
                {
                    if (!short.TryParse(TxtAnio.Text.Trim(), out short anioIngresado))
                    {
                        MessageBox.Show("El año debe ser un número válido.",
                            "Dato incorrecto", MessageBoxButton.OK, MessageBoxImage.Warning);

                        TxtAnio.Focus();
                        return;
                    }

                    anio = anioIngresado;
                }


                // -------------------------------------------------
                // CREAR OBJETO
                // -------------------------------------------------

                Moto motoData = new Moto
                {
                    IdMoto = _idMotoActual,

                    IdCliente = _idClienteSeleccionado,

                    Marca = TxtMarca.Text.Trim(),

                    Modelo = TxtModelo.Text.Trim(),

                    Anio = anio,

                    Patente = TxtPatente.Text.Trim(),

                    NroMotor = TxtMotor.Text.Trim(),

                    NroChasis = TxtChasis.Text.Trim(),

                    Observaciones =
                        TxtObservaciones.Text.Trim()
                };


                // -------------------------------------------------
                // NUEVA MOTO
                // -------------------------------------------------

                if (_idMotoActual == 0)
                {
                    _repo.AddMoto(motoData);

                    MessageBox.Show(
                        "La moto se registró correctamente.",
                        "Registro exitoso",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }


                // -------------------------------------------------
                // EDITAR MOTO
                // -------------------------------------------------

                else
                {
                    _repo.UpdateMoto(motoData);

                    MessageBox.Show(
                        "Los datos de la moto se actualizaron correctamente.",
                        "Actualización exitosa",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }


                // -------------------------------------------------
                // VOLVER A MOTOS
                // -------------------------------------------------

                MainWindow? mainWindow =
                    Window.GetWindow(this) as MainWindow;


                if (mainWindow != null)
                {
                    mainWindow.ContentFrame.Navigate(
                        new MotosView());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ocurrió un error al guardar la moto.\n\n"
                    + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}