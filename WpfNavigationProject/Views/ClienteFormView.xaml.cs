using System;
using System.Windows;
using System.Windows.Controls;
using WpfNavigationProject.Models;
using WpfNavigationProject.DataAccess;
using Microsoft.Data.SqlClient;

namespace WpfNavigationProject.Views
{
    public partial class ClienteFormView : UserControl
    {
        // 1. Instanciamos el repositorio
        private ClienteRepository _repo = new ClienteRepository();
        private int _idClienteActual;

        public ClienteFormView(int idCliente)
        {
            InitializeComponent();
            _idClienteActual = idCliente;

            // Si el ID es mayor a 0, estamos editando
            if (_idClienteActual > 0)
            {
                PrellenarFormulario();
                BtnCargar.Content = "Actualizar Cliente"; // Cambiamos el texto del botón
            }
        }

        private void PrellenarFormulario()
        {
            try
            {
                // Buscamos los datos en la base de datos
                Cliente cliente = _repo.GetClienteById(_idClienteActual);

                if (cliente != null)
                {
                    // Llenamos los TextBox con los datos recuperados
                    TxtNombre.Text = cliente.Nombre;
                    TxtDNI.Text = cliente.DNI;
                    TxtCelular.Text = cliente.Telefono;
                    TxtDireccion.Text = cliente.Direccion;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar datos: {ex.Message}");
            }
        }

        private void BtnCargar_Click(object sender, RoutedEventArgs e)
        {
            // 1. Validaciones básicas
            if (string.IsNullOrWhiteSpace(TxtNombre.Text) || string.IsNullOrWhiteSpace(TxtDNI.Text))
            {
                MessageBox.Show("El Nombre y el DNI son obligatorios.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Creamos el objeto con los datos actuales del formulario
            Cliente clienteData = new Cliente
            {
                IdCliente = _idClienteActual,
                Nombre = TxtNombre.Text,
                DNI = TxtDNI.Text,
                Telefono = TxtCelular.Text,
                Direccion = TxtDireccion.Text
            };

            try
            {
                if (_idClienteActual == 0)
                {
                    // MODO CREACIÓN
                    _repo.AddCliente(clienteData);
                    MessageBox.Show("Cliente registrado con éxito.", "Éxito");
                }
                else
                {
                    // MODO EDICIÓN
                    _repo.UpdateCliente(clienteData);
                    MessageBox.Show("Cliente actualizado con éxito.", "Éxito");
                }

                // 3. Volvemos a la lista de clientes después de guardar
                NavegarAListaClientes();
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error de base de datos: {ex.Message}", "Error SQL");
            }
        }

        private void NavegarAListaClientes()
        {
            MainWindow mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.ContentFrame.Navigate(new ClientesView());
            }
        }
    }
}