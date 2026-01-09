using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using WireModelingPlugin.Models;
using WireModelingPlugin.Services;

namespace WireModelingPlugin.UI;

/// <summary>
/// Dialog for viewing and exporting material takeoff reports.
/// </summary>
public partial class MaterialTakeoffDialog : Window
{
    private readonly Dictionary<string, double> _lengthsByType;
    private readonly Dictionary<WireGauge, double> _lengthsByGauge;
    private readonly double _totalLength;

    public ExportFormat SelectedFormat { get; private set; } = ExportFormat.Excel;
    public string SelectedFilePath { get; private set; } = string.Empty;

    public MaterialTakeoffDialog(
        Dictionary<string, double> lengthsByType,
        Dictionary<WireGauge, double> lengthsByGauge,
        double totalLength)
    {
        InitializeComponent();

        _lengthsByType = lengthsByType;
        _lengthsByGauge = lengthsByGauge;
        _totalLength = totalLength;

        LoadTakeoffData();
        UpdateSummary();
    }

    private void LoadTakeoffData()
    {
        // Load by type
        var byTypeItems = _lengthsByType
            .OrderByDescending(kv => kv.Value)
            .Select(kv => new TypeTakeoffItem
            {
                Type = kv.Key,
                Length = kv.Value,
                LengthMeters = kv.Value * 0.3048
            }).ToList();

        ByTypeDataGrid.ItemsSource = byTypeItems;

        // Load by gauge
        var byGaugeItems = _lengthsByGauge
            .OrderBy(kv => kv.Key.GetNumericValue())
            .Select(kv => new GaugeTakeoffItem
            {
                Gauge = kv.Key.GetDisplayName(),
                Length = kv.Value,
                LengthMeters = kv.Value * 0.3048
            }).ToList();

        ByGaugeDataGrid.ItemsSource = byGaugeItems;
    }

    private void UpdateSummary()
    {
        var totalMeters = _totalLength * 0.3048;
        TotalLengthText.Text = $"Total Wire: {_totalLength:F1} ft ({totalMeters:F1} m)";
    }

    private void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        // Get selected format
        var formatItem = FormatCombo.SelectedItem as ComboBoxItem;
        var formatText = formatItem?.Content.ToString() ?? "Excel";

        SelectedFormat = formatText switch
        {
            "PDF" => ExportFormat.PDF,
            "CSV" => ExportFormat.CSV,
            _ => ExportFormat.Excel
        };

        // Show save dialog
        var filter = SelectedFormat switch
        {
            ExportFormat.PDF => "PDF files (*.pdf)|*.pdf",
            ExportFormat.CSV => "CSV files (*.csv)|*.csv",
            _ => "Excel files (*.xlsx)|*.xlsx"
        };

        var extension = SelectedFormat switch
        {
            ExportFormat.PDF => ".pdf",
            ExportFormat.CSV => ".csv",
            _ => ".xlsx"
        };

        var saveDialog = new SaveFileDialog
        {
            Filter = filter,
            DefaultExt = extension,
            FileName = $"MaterialTakeoff_{DateTime.Now:yyyyMMdd}"
        };

        if (saveDialog.ShowDialog() == true)
        {
            SelectedFilePath = saveDialog.FileName;
            DialogResult = true;
            Close();
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private class TypeTakeoffItem
    {
        public string Type { get; set; } = string.Empty;
        public double Length { get; set; }
        public double LengthMeters { get; set; }
    }

    private class GaugeTakeoffItem
    {
        public string Gauge { get; set; } = string.Empty;
        public double Length { get; set; }
        public double LengthMeters { get; set; }
    }
}
