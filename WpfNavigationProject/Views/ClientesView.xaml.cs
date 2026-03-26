using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;
using WpfNavigationProject.DataAccess;
using WpfNavigationProject.Models;

// Importante: Necesitas la referencia a la MainWindow para la navegación
using WpfNavigationProject.Views;

namespace WpfNavigationProject.Views
{
    /// <summary>
    /// Lógica de interacción para ClientesView.xaml
    /// </summary>
    public partial class ClientesView : UserControl
    {
        // Instancia del Repositorio de Clientes
        private ClienteRepository _clienteRepository = new ClienteRepository();

        public ClientesView()
        {
            InitializeComponent();
            // Llama a la función de carga al iniciar la vista
            CargarDatosClientes();
        }

        /// <summary>
        /// Obtiene todos los clientes de la base de datos y los enlaza al DataGrid.
        /// </summary>
        public void CargarDatosClientes()
        {
            try
            {
                // 1. Obtener los datos
                List<Cliente> clientes = _clienteRepository.GetAllClientes();

                // 2. Conectar al DataGrid
                ClientesDataGrid.ItemsSource = clientes;
            }
            catch (System.Configuration.ConfigurationErrorsException ex)
            {
                MessageBox.Show($"Error de configuración: Revisa tu App.config. {ex.Message}", "Error de Configuración");
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error de base de datos: Asegúrate de que SQL Server esté corriendo. {ex.Message}", "Error SQL");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error inesperado: {ex.Message}", "Error General");
            }
        }

        // --- Lógica de Navegación y CRUD ---

        /// <summary>
        /// Centraliza la navegación al formulario de creación/edición.
        /// </summary>
        /// <param name="idClienteAEditar">ID del cliente (0 = Nuevo, > 0 = Editar).</param>
        private void NavegarAFormulario(int idClienteAEditar)
        {
            // Obtener la ventana principal (MainWindow)
            MainWindow mainWindow = Window.GetWindow(this) as MainWindow;

            if (mainWindow != null)
            {
                // Crear la instancia del formulario, pasando el ID
                ClienteFormView formulario = new ClienteFormView(idClienteAEditar);

                // Navegar en el Frame principal (ContentFrame)
                mainWindow.ContentFrame.Navigate(formulario);
            }
        }

        // Manejador del botón "+ Nuevo Cliente" (navegación para CREAR)
        private void BtnNuevoCliente_Click(object sender, RoutedEventArgs e)
        {
            // ID = 0 indica modo CREACIÓN
            NavegarAFormulario(0);
        }

        // Manejador de los botones "Editar" dentro del DataGrid (navegación para EDITAR)
        private void BtnEditar_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int idCliente)
            {
                // El ID > 0 indica modo EDICIÓN
                NavegarAFormulario(idCliente);
            }
        }

        // Manejador de los botones "Eliminar" dentro del DataGrid (DELETE)
        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            // 1. Obtenemos el ID del cliente desde el Tag del botón
            if (sender is Button button && button.Tag is int idCliente)
            {
                // 2. Preguntamos al usuario si está seguro (¡Fundamental en DELETE!)
                MessageBoxResult resultado = MessageBox.Show(
                    "¿Estás seguro de que deseas eliminar este cliente? Esta acción no se puede deshacer.",
                    "Confirmar Eliminación",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (resultado == MessageBoxResult.Yes)
                {
                    try
                    {
                        // 3. Llamamos al repositorio para borrar
                        _clienteRepository.DeleteCliente(idCliente);

                        // 4. Avisamos que salió bien
                        MessageBox.Show("Cliente eliminado correctamente.", "Éxito");

                        // 5. ¡IMPORTANTE! Refrescamos el DataGrid para que el cliente desaparezca de la lista
                        CargarDatosClientes();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al intentar eliminar: {ex.Message}", "Error");
                    }
                }
            }
        }

        // NOTA: El método CargarDatosParaEdicion() que tenías antes está obsoleto aquí.
        // La lógica de carga para la edición se maneja directamente en ClienteFormView.
    }
}