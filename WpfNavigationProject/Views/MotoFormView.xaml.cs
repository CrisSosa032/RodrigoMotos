using System;
using System.Windows;
using System.Windows.Controls;
using WpfNavigationProject.Models;
using WpfNavigationProject.DataAccess;
using Microsoft.Data.SqlClient;

namespace WpfNavigationProject.Views
{
    public partial class MotoFormView : UserControl
    {
        // Repositorio de motos
        private MotoRepository _repo = new MotoRepository();

        // ID de la moto que estamos editando.
        // Si es 0, significa que estamos creando una nueva.
        private int _idMotoActual;

        public MotoFormView(int idMoto)
        {
            InitializeComponent();

            _idMotoActual = idMoto;

            // Si el ID es mayor a 0, estamos editando.
            if (_idMotoActual > 0)
            {
                PrellenarFormulario();

                BtnCargarMoto.Content = "Actualizar Moto";
            }
        }

        private void PrellenarFormulario()
        {
            try
            {
                // Buscamos los datos en la base de datos.
                Moto? moto = _repo.GetMotoById(_idMotoActual);

                if (moto != null)
                {
                    // Llenamos los TextBox con los datos recuperados.
                    TxtIdCliente.Text = moto.IdCliente.ToString();
                    TxtMarca.Text = moto.Marca;
                    TxtModelo.Text = moto.Modelo;
                    TxtAnio.Text = moto.Anio.ToString();
                    TxtPatente.Text = moto.Patente;
                    TxtMotor.Text = moto.NroMotor;
                    TxtChasis.Text = moto.NroChasis;
                    TxtObservaciones.Text = moto.Observaciones;
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

        private void BtnCargarMoto_Click(
            object sender,
            RoutedEventArgs e)
        {
            // 1. Validaciones básicas.
            if (string.IsNullOrWhiteSpace(TxtIdCliente.Text) ||
                string.IsNullOrWhiteSpace(TxtMarca.Text) ||
                string.IsNullOrWhiteSpace(TxtPatente.Text))
            {
                MessageBox.Show(
                    "El ID del Cliente, la Marca y la Patente son obligatorios.",
                    "Validación",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            // Validamos que el ID del cliente sea realmente un número.
            if (!int.TryParse(
                    TxtIdCliente.Text,
                    out int idClienteValido))
            {
                MessageBox.Show(
                    "El ID del Cliente debe ser un número válido.",
                    "Error de formato",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            // Intentamos convertir el año.
            int.TryParse(
                TxtAnio.Text,
                out int anioValido);

            // 2. Creamos el objeto con los datos actuales.
            Moto motoData = new Moto
            {
                IdMoto = _idMotoActual,

                IdCliente = idClienteValido,

                Marca = TxtMarca.Text,

                Modelo = TxtModelo.Text,

                Anio = (short)anioValido,

                Patente = TxtPatente.Text,

                NroMotor = TxtMotor.Text,

                NroChasis = TxtChasis.Text,

                Observaciones = TxtObservaciones.Text
            };

            try
            {
                if (_idMotoActual == 0)
                {
                    // =====================================================
                    // MODO CREACIÓN
                    // =====================================================

                    // AddMoto devuelve el IdMoto generado por SQL Server.
                    int nuevoIdMoto = _repo.AddMoto(motoData);

                    if (nuevoIdMoto <= 0)
                    {
                        MessageBox.Show(
                            "La moto fue registrada, pero no se pudo obtener su ID.",
                            "Advertencia",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);

                        NavegarAListaMotos();
                        return;
                    }

                    // Preguntamos si desea agregar un servicio.
                    MessageBoxResult respuesta = MessageBox.Show(
                        "Moto registrada con éxito.\n\n" +
                        "¿Desea añadir un servicio a esta moto?",
                        "Moto registrada",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (respuesta == MessageBoxResult.Yes)
                    {
                        // Abrimos el formulario de servicio.
                        //
                        // nuevoIdMoto = ID generado por SQL Server.
                        // 1 = Estado "Ingresado".
                        MainWindow? mainWindow =
                            Window.GetWindow(this) as MainWindow;

                        if (mainWindow != null)
                        {
                            mainWindow.ContentFrame.Navigate(
                                new ServicioFormView(
                                    nuevoIdMoto,
                                    1));
                        }
                    }
                    else
                    {
                        // Si eligió NO, volvemos a MotosView.
                        NavegarAListaMotos();
                    }
                }
                else
                {
                    // =====================================================
                    // MODO EDICIÓN
                    // =====================================================

                    _repo.UpdateMoto(motoData);

                    MessageBox.Show(
                        "Moto actualizada con éxito.",
                        "Éxito",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    // Después de editar seguimos volviendo a MotosView.
                    NavegarAListaMotos();
                }
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
                    $"Ocurrió un error inesperado: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void NavegarAListaMotos()
        {
            MainWindow? mainWindow =
                Window.GetWindow(this) as MainWindow;

            if (mainWindow != null)
            {
                mainWindow.ContentFrame.Navigate(
                    new MotosView());
            }
        }
    }
}