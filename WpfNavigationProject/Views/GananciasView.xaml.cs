using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using WpfNavigationProject.DataAccess;
using WpfNavigationProject.Models;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using ClosedXML.Excel;
using Microsoft.Win32;

namespace WpfNavigationProject.Views
{
    public partial class GananciasView : UserControl
    {
        private readonly GananciasRepository _repository;

        // Cultura argentina para mostrar valores monetarios.
        private readonly CultureInfo _culturaArgentina =
            new CultureInfo("es-AR");

        // ============================================================
        // GANANCIAS ACTUALMENTE MOSTRADAS / FILTRADAS
        // ============================================================

        private List<Ganancia> _gananciasActuales =
            new List<Ganancia>();

        // ============================================================
        // DATOS COMPLETOS PARA EXPORTACIÓN
        // ============================================================

        private List<GananciaReporte> _reporteActual =
            new List<GananciaReporte>();


        public GananciasView()
        {
            InitializeComponent();

            _repository = new GananciasRepository();

            Loaded += GananciasView_Loaded;
        }


        // ============================================================
        // AL CARGAR LA VISTA
        // ============================================================

        private void GananciasView_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            CargarGanancias();
        }


        // ============================================================
        // CARGAR TODAS LAS GANANCIAS
        // ============================================================

        private void CargarGanancias()
        {
            try
            {
                List<Ganancia> ganancias =
                    _repository.GetAllGanancias();

                _gananciasActuales = ganancias;

                DgGanancias.ItemsSource = ganancias;

                _reporteActual =
                    _repository.GetGananciasReporteFiltradas(
                        null,
                        null);

                decimal total =
                    _repository.GetTotalGanancias();

                TxtTotalGanancias.Text =
                    total.ToString(
                        "C2",
                        _culturaArgentina);

                ActualizarGrafico(ganancias);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"No se pudieron cargar las ganancias.\n\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // ============================================================
        // BOTÓN FILTRAR
        // ============================================================

        private void BtnFiltrar_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                DateTime? fechaDesde =
                    DpFechaDesde.SelectedDate;

                DateTime? fechaHasta =
                    DpFechaHasta.SelectedDate;

                if (fechaDesde.HasValue &&
                    fechaHasta.HasValue &&
                    fechaDesde.Value.Date > fechaHasta.Value.Date)
                {
                    MessageBox.Show(
                        "La fecha 'Desde' no puede ser posterior a la fecha 'Hasta'.",
                        "Rango de fechas incorrecto",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                List<Ganancia> ganancias =
                    _repository.GetGananciasFiltradas(
                        fechaDesde,
                        fechaHasta);

                _gananciasActuales = ganancias;

                DgGanancias.ItemsSource = ganancias;

                _reporteActual =
                    _repository.GetGananciasReporteFiltradas(
                        fechaDesde,
                        fechaHasta);

                decimal total =
                    _repository.GetTotalGananciasFiltradas(
                        fechaDesde,
                        fechaHasta);

                TxtTotalGanancias.Text =
                    total.ToString(
                        "C2",
                        _culturaArgentina);

                ActualizarGrafico(ganancias);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"No se pudieron filtrar las ganancias.\n\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // ============================================================
        // BOTÓN LIMPIAR
        // ============================================================

        private void BtnLimpiar_Click(
            object sender,
            RoutedEventArgs e)
        {
            DpFechaDesde.SelectedDate = null;
            DpFechaHasta.SelectedDate = null;

            CargarGanancias();
        }


        // ============================================================
        // ACTUALIZAR GRÁFICO
        // ============================================================

        private void ActualizarGrafico(
            List<Ganancia> ganancias)
        {
            if (ganancias == null ||
                ganancias.Count == 0)
            {
                ChartGanancias.Series =
                    Array.Empty<ISeries>();

                ChartGanancias.XAxes =
                    Array.Empty<Axis>();

                ChartGanancias.YAxes =
                    Array.Empty<Axis>();

                return;
            }

            var gananciasPorFecha =
                ganancias
                    .GroupBy(g => g.FechaGanancia.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new DateTimePoint(
                        g.Key,
                        (double)g.Sum(x => x.Monto)))
                    .ToArray();

            ChartGanancias.Series =
            new ISeries[]
            {
                new LineSeries<DateTimePoint>
                {
                    Name = "Ganancias",

                    Values = gananciasPorFecha,

                    GeometrySize = 9,

                    LineSmoothness = 0.45,

                    Stroke = new SolidColorPaint(
                        new SKColor(0, 122, 204))
                    {
                        StrokeThickness = 3
                    },

                    Fill = new SolidColorPaint(
                        new SKColor(0, 122, 204, 35)),

                    GeometryFill = new SolidColorPaint(
                        SKColors.White),

                    GeometryStroke = new SolidColorPaint(
                        new SKColor(0, 122, 204))
                    {
                        StrokeThickness = 3
                    }
                }
            };

            ChartGanancias.XAxes =
                new Axis[]
                {
                    new DateTimeAxis(
                        TimeSpan.FromDays(1),
                        date => date.ToString("dd/MM"))
                    {
                        LabelsRotation = 0,

                        LabelsPaint = new SolidColorPaint(
                            new SKColor(107, 114, 128)),

                        SeparatorsPaint = new SolidColorPaint(
                            new SKColor(229, 231, 235))
                    }
                };

            ChartGanancias.YAxes =
                new Axis[]
                {
                    new Axis
                    {
                        Labeler = value =>
                            value.ToString(
                                "C0",
                                _culturaArgentina),

                        LabelsPaint = new SolidColorPaint(
                            new SKColor(107, 114, 128)),

                        SeparatorsPaint = new SolidColorPaint(
                            new SKColor(229, 231, 235)),

                        MinStep = 1
                    }
                };
        }



        // ============================================================
        // EXPORTAR EXCEL
        // ============================================================

        private void BtnExportarExcel_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                // ----------------------------------------------------
                // VALIDAR DATOS
                // ----------------------------------------------------

                if (_reporteActual == null ||
                    _reporteActual.Count == 0)
                {
                    MessageBox.Show(
                        "No hay ganancias para exportar.",
                        "Exportar Excel",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    return;
                }


                // ----------------------------------------------------
                // DIÁLOGO GUARDAR
                // ----------------------------------------------------

                SaveFileDialog saveFileDialog =
                    new SaveFileDialog
                    {
                        Title = "Guardar reporte de ganancias",

                        Filter =
                            "Archivo Excel (*.xlsx)|*.xlsx",

                        DefaultExt = ".xlsx",

                        AddExtension = true,

                        FileName =
                            $"Ganancias_{DateTime.Now:yyyy-MM-dd}.xlsx"
                    };


                bool? resultado =
                    saveFileDialog.ShowDialog();


                if (resultado != true)
                {
                    return;
                }


                // ====================================================
                // CREAR LIBRO
                // ====================================================

                using (XLWorkbook workbook =
                    new XLWorkbook())
                {
                    IXLWorksheet worksheet =
                        workbook.Worksheets.Add("Ganancias");


                    // =================================================
                    // COLORES DEL REPORTE
                    // =================================================

                    XLColor azulPrincipal =
                        XLColor.FromHtml("#007ACC");

                    XLColor azulOscuro =
                        XLColor.FromHtml("#005A9E");

                    XLColor azulClaro =
                        XLColor.FromHtml("#EAF4FB");

                    XLColor grisTexto =
                        XLColor.FromHtml("#4B5563");

                    XLColor grisClaro =
                        XLColor.FromHtml("#F3F4F6");

                    XLColor grisBorde =
                        XLColor.FromHtml("#D1D5DB");

                    XLColor verde =
                        XLColor.FromHtml("#DCFCE7");

                    XLColor verdeTexto =
                        XLColor.FromHtml("#166534");


                    // =================================================
                    // TÍTULO PRINCIPAL
                    // =================================================

                    worksheet.Range("A1:G1").Merge();

                    worksheet.Cell("A1").Value =
                        "RODRIGO MOTOS";

                    worksheet.Cell("A1").Style.Font.Bold =
                        true;

                    worksheet.Cell("A1").Style.Font.FontSize =
                        20;

                    worksheet.Cell("A1").Style.Font.FontColor =
                        XLColor.White;

                    worksheet.Cell("A1").Style.Fill.BackgroundColor =
                        azulPrincipal;

                    worksheet.Cell("A1").Style.Alignment.Horizontal =
                        XLAlignmentHorizontalValues.Center;

                    worksheet.Cell("A1").Style.Alignment.Vertical =
                        XLAlignmentVerticalValues.Center;

                    worksheet.Row(1).Height = 34;


                    // =================================================
                    // SUBTÍTULO
                    // =================================================

                    worksheet.Range("A2:G2").Merge();

                    worksheet.Cell("A2").Value =
                        "REPORTE DE GANANCIAS";

                    worksheet.Cell("A2").Style.Font.Bold =
                        true;

                    worksheet.Cell("A2").Style.Font.FontSize =
                        13;

                    worksheet.Cell("A2").Style.Font.FontColor =
                        azulOscuro;

                    worksheet.Cell("A2").Style.Alignment.Horizontal =
                        XLAlignmentHorizontalValues.Center;

                    worksheet.Cell("A2").Style.Alignment.Vertical =
                        XLAlignmentVerticalValues.Center;

                    worksheet.Row(2).Height = 24;


                    // =================================================
                    // PERÍODO
                    // =================================================

                    string periodo;

                    if (DpFechaDesde.SelectedDate.HasValue &&
                        DpFechaHasta.SelectedDate.HasValue)
                    {
                        periodo =
                            $"Desde {DpFechaDesde.SelectedDate.Value:dd/MM/yyyy} " +
                            $"hasta {DpFechaHasta.SelectedDate.Value:dd/MM/yyyy}";
                    }
                    else if (DpFechaDesde.SelectedDate.HasValue)
                    {
                        periodo =
                            $"Desde {DpFechaDesde.SelectedDate.Value:dd/MM/yyyy}";
                    }
                    else if (DpFechaHasta.SelectedDate.HasValue)
                    {
                        periodo =
                            $"Hasta {DpFechaHasta.SelectedDate.Value:dd/MM/yyyy}";
                    }
                    else
                    {
                        periodo = "Todas las ganancias";
                    }


                    worksheet.Range("A3:G3").Merge();

                    worksheet.Cell("A3").Value =
                        $"Período: {periodo}";

                    worksheet.Cell("A3").Style.Font.FontColor =
                        grisTexto;

                    worksheet.Cell("A3").Style.Font.Italic =
                        true;

                    worksheet.Cell("A3").Style.Alignment.Horizontal =
                        XLAlignmentHorizontalValues.Center;


                    // =================================================
                    // FECHA DE GENERACIÓN
                    // =================================================

                    worksheet.Range("A4:G4").Merge();

                    worksheet.Cell("A4").Value =
                        $"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}";

                    worksheet.Cell("A4").Style.Font.FontSize =
                        9;

                    worksheet.Cell("A4").Style.Font.FontColor =
                        grisTexto;

                    worksheet.Cell("A4").Style.Alignment.Horizontal =
                        XLAlignmentHorizontalValues.Center;


                    // =================================================
                    // RESUMEN
                    // =================================================

                    int filaResumen = 6;


                    // -------------------------------------------------
                    // TARJETA REGISTROS
                    // -------------------------------------------------

                    worksheet.Range(
                        filaResumen,
                        1,
                        filaResumen + 1,
                        3)
                        .Merge();

                    worksheet.Cell(
                        filaResumen,
                        1).Value =
                        $"{_reporteActual.Count}\n REGISTROS";

                    worksheet.Cell(
                        filaResumen,
                        1).Style.Font.Bold =
                        true;

                    worksheet.Cell(
                        filaResumen,
                        1).Style.Font.FontSize =
                        14;

                    worksheet.Cell(
                        filaResumen,
                        1).Style.Font.FontColor =
                        azulOscuro;

                    worksheet.Cell(
                        filaResumen,
                        1).Style.Fill.BackgroundColor =
                        azulClaro;

                    worksheet.Cell(
                        filaResumen,
                        1).Style.Alignment.Horizontal =
                        XLAlignmentHorizontalValues.Center;

                    worksheet.Cell(
                        filaResumen,
                        1).Style.Alignment.Vertical =
                        XLAlignmentVerticalValues.Center;


                    // -------------------------------------------------
                    // TARJETA TOTAL
                    // -------------------------------------------------

                    decimal totalReporte =
                        _reporteActual.Sum(
                            x => x.Precio);

                    worksheet.Range(
                        filaResumen,
                        5,
                        filaResumen + 1,
                        7)
                        .Merge();

                    worksheet.Cell(
                        filaResumen,
                        5).Value =
                        $"{totalReporte.ToString("C2", _culturaArgentina)}\n TOTAL GANANCIAS";

                    worksheet.Cell(
                        filaResumen,
                        5).Style.Font.Bold =
                        true;

                    worksheet.Cell(
                        filaResumen,
                        5).Style.Font.FontSize =
                        14;

                    worksheet.Cell(
                        filaResumen,
                        5).Style.Font.FontColor =
                        XLColor.White;

                    worksheet.Cell(
                        filaResumen,
                        5).Style.Fill.BackgroundColor =
                        azulPrincipal;

                    worksheet.Cell(
                        filaResumen,
                        5).Style.Alignment.Horizontal =
                        XLAlignmentHorizontalValues.Center;

                    worksheet.Cell(
                        filaResumen,
                        5).Style.Alignment.Vertical =
                        XLAlignmentVerticalValues.Center;


                    worksheet.Row(filaResumen).Height =
                        25;

                    worksheet.Row(filaResumen + 1).Height =
                        25;


                    // =================================================
                    // ENCABEZADOS
                    // =================================================

                    int filaEncabezados = 9;

                    worksheet.Cell(
                        filaEncabezados,
                        1).Value = "Fecha";

                    worksheet.Cell(
                        filaEncabezados,
                        2).Value = "Cliente";

                    worksheet.Cell(
                        filaEncabezados,
                        3).Value = "Marca";

                    worksheet.Cell(
                        filaEncabezados,
                        4).Value = "Modelo";

                    worksheet.Cell(
                        filaEncabezados,
                        5).Value = "Estado";

                    worksheet.Cell(
                        filaEncabezados,
                        6).Value = "Detalle";

                    worksheet.Cell(
                        filaEncabezados,
                        7).Value = "Precio";


                    // =================================================
                    // ESTILO ENCABEZADOS
                    // =================================================

                    IXLRange rangoEncabezados =
                        worksheet.Range(
                            filaEncabezados,
                            1,
                            filaEncabezados,
                            7);

                    rangoEncabezados.Style.Font.Bold =
                        true;

                    rangoEncabezados.Style.Font.FontColor =
                        XLColor.White;

                    rangoEncabezados.Style.Fill.BackgroundColor =
                        azulOscuro;

                    rangoEncabezados.Style.Alignment.Horizontal =
                        XLAlignmentHorizontalValues.Center;

                    rangoEncabezados.Style.Alignment.Vertical =
                        XLAlignmentVerticalValues.Center;

                    rangoEncabezados.Style.Border.BottomBorder =
                        XLBorderStyleValues.Thin;

                    rangoEncabezados.Style.Border.BottomBorderColor =
                        azulPrincipal;

                    worksheet.Row(filaEncabezados).Height =
                        24;


                    // =================================================
                    // CARGAR DATOS
                    // =================================================

                    int fila = filaEncabezados + 1;

                    foreach (GananciaReporte ganancia
                        in _reporteActual)
                    {
                        // ---------------------------------------------
                        // DATOS
                        // ---------------------------------------------

                        worksheet.Cell(fila, 1).Value =
                            ganancia.Fecha;

                        worksheet.Cell(fila, 2).Value =
                            ganancia.Cliente;

                        worksheet.Cell(fila, 3).Value =
                            ganancia.Marca;

                        worksheet.Cell(fila, 4).Value =
                            ganancia.Modelo;

                        worksheet.Cell(fila, 5).Value =
                            ganancia.Estado;

                        worksheet.Cell(fila, 6).Value =
                            ganancia.Detalle;

                        worksheet.Cell(fila, 7).Value =
                            ganancia.Precio;


                        // ---------------------------------------------
                        // FILAS ALTERNADAS
                        // ---------------------------------------------

                        if ((fila - filaEncabezados) % 2 == 0)
                        {
                            worksheet.Range(
                                fila,
                                1,
                                fila,
                                7)
                                .Style.Fill.BackgroundColor =
                                grisClaro;
                        }


                        // ---------------------------------------------
                        // BORDES
                        // ---------------------------------------------

                        worksheet.Range(
                            fila,
                            1,
                            fila,
                            7)
                            .Style.Border.BottomBorder =
                            XLBorderStyleValues.Hair;

                        worksheet.Range(
                            fila,
                            1,
                            fila,
                            7)
                            .Style.Border.BottomBorderColor =
                            grisBorde;


                        // ---------------------------------------------
                        // ESTADO PAGADO
                        // ---------------------------------------------

                        if (string.Equals(
                            ganancia.Estado,
                            "Pagado",
                            StringComparison.OrdinalIgnoreCase))
                        {
                            worksheet.Cell(fila, 5)
                                .Style.Fill.BackgroundColor =
                                verde;

                            worksheet.Cell(fila, 5)
                                .Style.Font.FontColor =
                                verdeTexto;

                            worksheet.Cell(fila, 5)
                                .Style.Font.Bold =
                                true;
                        }

                        fila++;
                    }


                    // =================================================
                    // FORMATO DE FECHA
                    // =================================================

                    if (fila > filaEncabezados + 1)
                    {
                        worksheet.Range(
                            filaEncabezados + 1,
                            1,
                            fila - 1,
                            1)
                            .Style.NumberFormat.Format =
                            "dd/MM/yyyy";


                        // =================================================
                        // FORMATO MONETARIO
                        // =================================================

                        worksheet.Range(
                            filaEncabezados + 1,
                            7,
                            fila - 1,
                            7)
                            .Style.NumberFormat.Format =
                            "$ #,##0.00";


                        // =================================================
                        // ALINEACIONES
                        // =================================================

                        worksheet.Range(
                            filaEncabezados + 1,
                            1,
                            fila - 1,
                            1)
                            .Style.Alignment.Horizontal =
                            XLAlignmentHorizontalValues.Center;

                        worksheet.Range(
                            filaEncabezados + 1,
                            5,
                            fila - 1,
                            5)
                            .Style.Alignment.Horizontal =
                            XLAlignmentHorizontalValues.Center;

                        worksheet.Range(
                            filaEncabezados + 1,
                            7,
                            fila - 1,
                            7)
                            .Style.Alignment.Horizontal =
                            XLAlignmentHorizontalValues.Right;
                    }


                    // =================================================
                    // TOTAL FINAL
                    // =================================================

                    int filaTotal =
                        fila + 1;

                    worksheet.Range(
                        filaTotal,
                        1,
                        filaTotal,
                        6)
                        .Merge();

                    worksheet.Cell(
                        filaTotal,
                        1).Value =
                        "TOTAL GANANCIAS";

                    worksheet.Cell(
                        filaTotal,
                        1).Style.Font.Bold =
                        true;

                    worksheet.Cell(
                        filaTotal,
                        1).Style.Font.FontSize =
                        12;

                    worksheet.Cell(
                        filaTotal,
                        1).Style.Font.FontColor =
                        XLColor.White;

                    worksheet.Cell(
                        filaTotal,
                        1).Style.Fill.BackgroundColor =
                        azulOscuro;

                    worksheet.Cell(
                        filaTotal,
                        1).Style.Alignment.Horizontal =
                        XLAlignmentHorizontalValues.Right;

                    worksheet.Cell(
                        filaTotal,
                        1).Style.Alignment.Vertical =
                        XLAlignmentVerticalValues.Center;


                    worksheet.Cell(
                        filaTotal,
                        7).FormulaA1 =
                        $"SUM(G{filaEncabezados + 1}:G{fila - 1})";

                    worksheet.Cell(
                        filaTotal,
                        7).Style.Font.Bold =
                        true;

                    worksheet.Cell(
                        filaTotal,
                        7).Style.Font.FontSize =
                        12;

                    worksheet.Cell(
                        filaTotal,
                        7).Style.Font.FontColor =
                        XLColor.White;

                    worksheet.Cell(
                        filaTotal,
                        7).Style.Fill.BackgroundColor =
                        azulPrincipal;

                    worksheet.Cell(
                        filaTotal,
                        7).Style.NumberFormat.Format =
                        "$ #,##0.00";

                    worksheet.Cell(
                        filaTotal,
                        7).Style.Alignment.Horizontal =
                        XLAlignmentHorizontalValues.Right;

                    worksheet.Row(filaTotal).Height =
                        27;


                    // =================================================
                    // AUTOFILTRO
                    // =================================================

                    worksheet.Range(
                        filaEncabezados,
                        1,
                        fila - 1,
                        7)
                        .SetAutoFilter();


                    // =================================================
                    // CONGELAR ENCABEZADOS
                    // =================================================

                    worksheet.SheetView.FreezeRows(
                        filaEncabezados);


                    // =================================================
                    // ANCHOS DE COLUMNAS
                    // =================================================

                    worksheet.Column(1).Width = 13;
                    worksheet.Column(2).Width = 28;
                    worksheet.Column(3).Width = 16;
                    worksheet.Column(4).Width = 18;
                    worksheet.Column(5).Width = 18;
                    worksheet.Column(6).Width = 45;
                    worksheet.Column(7).Width = 18;


                    // =================================================
                    // AJUSTAR ALTO DE FILAS
                    // =================================================

                    if (fila > filaEncabezados + 1)
                    {
                        worksheet.Range(
                            filaEncabezados + 1,
                            1,
                            fila - 1,
                            7)
                            .Style.Alignment.Vertical =
                            XLAlignmentVerticalValues.Center;
                    }


                    // =================================================
                    // PIE DEL REPORTE
                    // =================================================

                    int filaPie =
                        filaTotal + 2;

                    worksheet.Range(
                        filaPie,
                        1,
                        filaPie,
                        7)
                        .Merge();

                    worksheet.Cell(
                        filaPie,
                        1).Value =
                        "Rodrigo Motos · Sistema de gestión de taller";

                    worksheet.Cell(
                        filaPie,
                        1).Style.Font.FontSize =
                        9;

                    worksheet.Cell(
                        filaPie,
                        1).Style.Font.FontColor =
                        grisTexto;

                    worksheet.Cell(
                        filaPie,
                        1).Style.Alignment.Horizontal =
                        XLAlignmentHorizontalValues.Center;


                    // =================================================
                    // CONFIGURACIÓN DE PÁGINA
                    // =================================================

                    worksheet.PageSetup.PageOrientation =
                        XLPageOrientation.Landscape;

                    worksheet.PageSetup.PaperSize =
                        XLPaperSize.A4Paper;

                    worksheet.PageSetup.Margins.Top =
                        0.5;

                    worksheet.PageSetup.Margins.Bottom =
                        0.5;

                    worksheet.PageSetup.Margins.Left =
                        0.4;

                    worksheet.PageSetup.Margins.Right =
                        0.4;


                    // =================================================
                    // GUARDAR
                    // =================================================

                    workbook.SaveAs(
                        saveFileDialog.FileName);
                }


                // =====================================================
                // CONFIRMACIÓN
                // =====================================================

                MessageBox.Show(
                    "El reporte de ganancias se exportó correctamente.",
                    "Exportar Excel",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"No se pudo exportar el archivo Excel.\n\n{ex.Message}",
                    "Error al exportar",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
