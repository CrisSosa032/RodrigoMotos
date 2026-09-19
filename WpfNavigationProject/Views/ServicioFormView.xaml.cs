using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;
using WpfNavigationProject.DataAccess;
using WpfNavigationProject.Models;

namespace WpfNavigationProject.Views
{
    public partial class ServicioFormView : UserControl
    {
        private readonly ServiciosRepository _serviciosRepository;
        private readonly ClienteRepository _clienteRepository;
        private readonly MotoRepository _motoRepository;

        private int _idServicioActual = 0;

        private List<Cliente> _clientes = new List<Cliente>();
        private List<Moto> _motosCliente = new List<Moto>();

        private int _idClienteSeleccionado = 0;
        private int _idMotoSeleccionada = 0;

        private bool _seleccionandoCliente = false;

        public ServicioFormView(int idServicio = 0)
        {
            InitializeComponent();

            _serviciosRepository = new ServiciosRepository();
            _clienteRepository = new ClienteRepository();
            _motoRepository = new MotoRepository();

            _idServicioActual = idServicio;

            CargarClientes();
            CargarEstados();

            if (_idServicioActual > 0)
            {
                CargarServicio();
            }
            else
            {
                SeleccionarEstadoIngresado();
            }
        }

        // ============================================================
        // CLIENTES
        // ============================================================

        private void CargarClientes()
        {
            try
            {
                _clientes = _clienteRepository.GetAllClientes();

                LstClientes.ItemsSource = _clientes;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudieron cargar los clientes.\n\n" + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void TxtBuscarCliente_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            if (_seleccionandoCliente)
                return;

            string texto = TxtBuscarCliente.Text.Trim();

            LstMotos.ItemsSource = null;
            LstMotos.Visibility = Visibility.Collapsed;

            TxtTituloMotos.Visibility = Visibility.Collapsed;

            LstMotos.SelectedItem = null;

            TxtMotoSeleccionada.Text = "Seleccioná una moto";

            _motosCliente = new List<Moto>();

            _idClienteSeleccionado = 0;
            _idMotoSeleccionada = 0;

            if (string.IsNullOrWhiteSpace(texto))
            {
                LstClientes.ItemsSource = _clientes;

                LstClientes.Visibility =
                    _clientes.Count > 0
                        ? Visibility.Visible
                        : Visibility.Collapsed;

                return;
            }

            var resultados = _clientes
                .Where(c =>
                    (!string.IsNullOrWhiteSpace(c.Nombre) &&
                     c.Nombre.Contains(
                         texto,
                         StringComparison.OrdinalIgnoreCase))
                    ||
                    (!string.IsNullOrWhiteSpace(c.DNI) &&
                     c.DNI.Contains(
                         texto,
                         StringComparison.OrdinalIgnoreCase)))
                .ToList();

            LstClientes.ItemsSource = resultados;

            LstClientes.Visibility =
                resultados.Count > 0
                    ? Visibility.Visible
                    : Visibility.Collapsed;
        }

        private void LstClientes_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (LstClientes.SelectedItem is not Cliente cliente)
                return;

            _idClienteSeleccionado = cliente.IdCliente;

            _seleccionandoCliente = true;

            TxtBuscarCliente.Text = cliente.Nombre;

            _seleccionandoCliente = false;

            LstClientes.Visibility = Visibility.Collapsed;

            CargarMotosCliente();
        }

        // ============================================================
        // MOTOS DEL CLIENTE
        // ============================================================

        private void CargarMotosCliente()
        {
            if (_idClienteSeleccionado <= 0)
                return;

            try
            {
                _motosCliente =
                    _motoRepository.GetMotosByCliente(
                        _idClienteSeleccionado);

                LstMotos.ItemsSource = null;
                LstMotos.ItemsSource = _motosCliente;

                LstMotos.SelectedItem = null;

                _idMotoSeleccionada = 0;

                TxtMotoSeleccionada.Text = "Seleccioná una moto";

                if (_motosCliente.Count > 0)
                {
                    TxtTituloMotos.Visibility = Visibility.Visible;
                    LstMotos.Visibility = Visibility.Visible;
                }
                else
                {
                    TxtTituloMotos.Visibility = Visibility.Collapsed;
                    LstMotos.Visibility = Visibility.Collapsed;

                    MessageBox.Show(
                        "El cliente seleccionado no tiene motos registradas.",
                        "Sin motos",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudieron cargar las motos del cliente.\n\n" +
                    ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void LstMotos_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (LstMotos.SelectedItem is not Moto moto)
                return;

            _idMotoSeleccionada = moto.IdMoto;

            TxtMotoSeleccionada.Text =
                $"{moto.Marca} {moto.Modelo} - {moto.Patente}";
        }

        // ============================================================
        // LIMPIAR CLIENTE / MOTO
        // ============================================================

        private void BtnLimpiarCliente_Click(
            object sender,
            RoutedEventArgs e)
        {
            _seleccionandoCliente = true;

            TxtBuscarCliente.Clear();

            _seleccionandoCliente = false;

            LstClientes.SelectedItem = null;

            LstClientes.ItemsSource = _clientes;

            LstClientes.Visibility =
                _clientes.Count > 0
                    ? Visibility.Visible
                    : Visibility.Collapsed;

            LstMotos.ItemsSource = null;
            LstMotos.SelectedItem = null;
            LstMotos.Visibility = Visibility.Collapsed;

            TxtTituloMotos.Visibility = Visibility.Collapsed;

            TxtMotoSeleccionada.Text = "Seleccioná una moto";

            _motosCliente = new List<Moto>();

            _idClienteSeleccionado = 0;
            _idMotoSeleccionada = 0;
        }

        // ============================================================
        // ESTADOS
        // ============================================================

        private void CargarEstados()
        {
            List<EstadoServicioItem> estados =
                new List<EstadoServicioItem>();

            string sql = @"
                SELECT
                    IdEstado,
                    NombreEstado
                FROM EstadosServicio
                ORDER BY IdEstado";

            using (SqlConnection connection =
                   DbHelper.CreateConnection())
            using (SqlCommand command =
                   new SqlCommand(sql, connection))
            {
                try
                {
                    connection.Open();

                    using (SqlDataReader reader =
                           command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            estados.Add(
                                new EstadoServicioItem
                                {
                                    IdEstado =
                                        Convert.ToInt32(
                                            reader["IdEstado"]),

                                    NombreEstado =
                                        reader["NombreEstado"]
                                            .ToString() ?? string.Empty
                                });
                        }
                    }

                    CmbEstado.ItemsSource = estados;
                    CmbEstado.DisplayMemberPath = "NombreEstado";
                    CmbEstado.SelectedValuePath = "IdEstado";
                }
                catch (SqlException ex)
                {
                    MessageBox.Show(
                        "No se pudieron cargar los estados del servicio.\n\n" +
                        ex.Message,
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private void SeleccionarEstadoIngresado()
        {
            CmbEstado.SelectedValue = 1;
        }

        // ============================================================
        // CARGAR SERVICIO PARA EDITAR
        // ============================================================

        private void CargarServicio()
        {
            try
            {
                Servicios? servicio =
                    _serviciosRepository.GetServicioById(
                        _idServicioActual);

                if (servicio == null)
                {
                    MessageBox.Show(
                        "No se encontró el servicio seleccionado.",
                        "Servicio no encontrado",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                int idCliente =
                    _motoRepository.GetIdClienteByMoto(
                        servicio.IdMoto);

                Cliente? cliente =
                    _clienteRepository.GetClienteById(
                        idCliente);

                if (cliente == null)
                {
                    MessageBox.Show(
                        "No se encontró el cliente asociado al servicio.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                _idClienteSeleccionado = idCliente;

                _seleccionandoCliente = true;

                TxtBuscarCliente.Text = cliente.Nombre;

                _seleccionandoCliente = false;

                LstClientes.SelectedItem = null;
                LstClientes.Visibility = Visibility.Collapsed;

                CargarMotosCliente();

                Moto? motoSeleccionada =
                    _motosCliente.FirstOrDefault(
                        m => m.IdMoto == servicio.IdMoto);

                if (motoSeleccionada != null)
                {
                    LstMotos.SelectedItem = motoSeleccionada;

                    _idMotoSeleccionada =
                        motoSeleccionada.IdMoto;

                    TxtMotoSeleccionada.Text =
                        $"{motoSeleccionada.Marca} " +
                        $"{motoSeleccionada.Modelo} - " +
                        $"{motoSeleccionada.Patente}";
                }

                CmbEstado.SelectedValue =
                    servicio.IdEstado;

                TxtDetalle.Text =
                    servicio.Detalle ?? string.Empty;

                if (servicio.CostoEstimado.HasValue)
                {
                    TxtCosto.Text =
                        servicio.CostoEstimado
                            .Value
                            .ToString("0.00");
                }
                else
                {
                    TxtCosto.Text = string.Empty;
                }

                TxtTitulo.Text = "Editar servicio";
                BtnCargarServicio.Content = "Guardar cambios";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo cargar el servicio.\n\n" +
                    ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // ============================================================
        // GUARDAR / ACTUALIZAR SERVICIO
        // ============================================================

        private void BtnCargarServicio_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (_idClienteSeleccionado <= 0)
            {
                MessageBox.Show(
                    "Seleccioná un cliente.",
                    "Validación",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (_idMotoSeleccionada <= 0)
            {
                MessageBox.Show(
                    "Seleccioná una moto.",
                    "Validación",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (CmbEstado.SelectedValue == null)
            {
                MessageBox.Show(
                    "Seleccioná un estado para el servicio.",
                    "Validación",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            int idEstado;

            try
            {
                idEstado =
                    Convert.ToInt32(
                        CmbEstado.SelectedValue);
            }
            catch
            {
                MessageBox.Show(
                    "El estado seleccionado no es válido.",
                    "Validación",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            decimal? costoEstimado = null;

            string textoCosto =
                TxtCosto.Text.Trim();

            if (!string.IsNullOrWhiteSpace(textoCosto))
            {
                textoCosto =
                    textoCosto.Replace(
                        "$",
                        string.Empty).Trim();

                if (!decimal.TryParse(
                        textoCosto,
                        out decimal costo))
                {
                    MessageBox.Show(
                        "Ingresá un costo válido.",
                        "Validación",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    TxtCosto.Focus();

                    return;
                }

                if (costo < 0)
                {
                    MessageBox.Show(
                        "El costo no puede ser negativo.",
                        "Validación",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    TxtCosto.Focus();

                    return;
                }

                costoEstimado = costo;
            }

            Servicios servicio =
                new Servicios
                {
                    IdServicio = _idServicioActual,
                    IdMoto = _idMotoSeleccionada,
                    IdEstado = idEstado,
                    Detalle = TxtDetalle.Text.Trim(),
                    CostoEstimado = costoEstimado
                };

            try
            {
                if (_idServicioActual == 0)
                {
                    _serviciosRepository.AddServicio(
                        servicio);

                    MessageBox.Show(
                        "El servicio fue registrado correctamente.",
                        "Servicio registrado",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    _serviciosRepository.UpdateServicio(
                        servicio);

                    MessageBox.Show(
                        "El servicio fue actualizado correctamente.",
                        "Servicio actualizado",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }

                if (Window.GetWindow(this) is MainWindow mainWindow)
                {
                    mainWindow.ContentFrame.Navigate(
                        new ServiciosView());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo guardar el servicio.\n\n" +
                    ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // ============================================================
        // MODELO AUXILIAR PARA ESTADOS
        // ============================================================

        public class EstadoServicioItem
        {
            public int IdEstado { get; set; }

            public string NombreEstado { get; set; }
                = string.Empty;
        }
    }
}