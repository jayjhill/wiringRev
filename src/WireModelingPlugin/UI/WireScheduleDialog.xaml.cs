using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using WireModelingPlugin.Models;
using WireModelingPlugin.Services;

namespace WireModelingPlugin.UI;

/// <summary>
/// Dialog for previewing and exporting wire pull schedules.
/// </summary>
public partial class WireScheduleDialog : Window
{
    private readonly List<Circuit> _circuits;
    private readonly List<WireSegment> _unassignedSegments;

    public ExportFormat SelectedFormat { get; private set; } = ExportFormat.Excel;
    public string SelectedFilePath { get; private set; } = string.Empty;

    public WireScheduleDialog(List<Circuit> circuits, List<WireSegment> unassignedSegments)
    {
        InitializeComponent();

        _circuits = circuits;
        _unassignedSegments = unassignedSegments;

        LoadScheduleData();
        UpdateSummary();
    }

    private void LoadScheduleData()
    {
        var scheduleItems = new List<ScheduleItem>();

        foreach (var circuit in _circuits)
        {
            var config = circuit.GetPrimaryConfiguration();
            scheduleItems.Add(new ScheduleItem
            {
                CircuitName = circuit.GetStandardName(),
                PanelName = circuit.PanelName,
                WireSpec = config?.GetSpecificationString() ?? "N/A",
                SystemType = circuit.SystemType.GetDisplayName(),
                Length = circuit.GetTotalLength(),
                Notes = circuit.Description ?? ""
            });
        }

        // Add unassigned segments
        foreach (var segment in _unassignedSegments)
        {
            scheduleItems.Add(new ScheduleItem
            {
                CircuitName = "(Unassigned)",
                PanelName = "",
                WireSpec = segment.Configuration.GetSpecificationString(),
                SystemType = "",
                Length = segment.TotalLength,
                Notes = segment.Notes ?? ""
            });
        }

        ScheduleDataGrid.ItemsSource = scheduleItems;
    }

    private void UpdateSummary()
    {
        var totalLength = _circuits.Sum(c => c.GetTotalLength()) +
                          _unassignedSegments.Sum(s => s.TotalLength);

        var circuitCount = _circuits.Count;
        var segmentCount = _circuits.Sum(c => c.Segments.Count) + _unassignedSegments.Count;

        SummaryText.Text = $"Total: {circuitCount} circuits, {segmentCount} segments, {totalLength:F1} ft of wire";
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
            FileName = $"WirePullSchedule_{DateTime.Now:yyyyMMdd}"
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

    private class ScheduleItem
    {
        public string CircuitName { get; set; } = string.Empty;
        public string PanelName { get; set; } = string.Empty;
        public string WireSpec { get; set; } = string.Empty;
        public string SystemType { get; set; } = string.Empty;
        public double Length { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
