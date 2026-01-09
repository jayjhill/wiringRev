using System.Windows;
using System.Windows.Controls;
using WireModelingPlugin.Models;
using WireModelingPlugin.Services;

namespace WireModelingPlugin.UI;

/// <summary>
/// Circuit Manager dialog for organizing circuits and wire assignments.
/// </summary>
public partial class CircuitManagerDialog : Window
{
    private readonly WireDataService _dataService;
    private readonly List<string> _panelNames;

    public CircuitManagerDialog(WireDataService dataService, List<string> panelNames)
    {
        InitializeComponent();

        _dataService = dataService;
        _panelNames = panelNames;

        InitializeFilters();
        LoadCircuits();
        LoadUnassignedSegments();
    }

    private void InitializeFilters()
    {
        // Panel filter
        PanelFilterCombo.Items.Add(new ComboBoxItem { Content = "(All Panels)" });
        foreach (var panel in _panelNames)
        {
            PanelFilterCombo.Items.Add(new ComboBoxItem { Content = panel, Tag = panel });
        }
        PanelFilterCombo.SelectedIndex = 0;

        // System type filter
        SystemFilterCombo.Items.Add(new ComboBoxItem { Content = "(All Systems)" });
        foreach (SystemType type in Enum.GetValues<SystemType>())
        {
            SystemFilterCombo.Items.Add(new ComboBoxItem
            {
                Content = type.GetDisplayName(),
                Tag = type
            });
        }
        SystemFilterCombo.SelectedIndex = 0;
    }

    private void LoadCircuits()
    {
        var circuits = _dataService.GetAllCircuits();

        // Apply panel filter
        if (PanelFilterCombo.SelectedItem is ComboBoxItem panelItem && panelItem.Tag is string panelName)
        {
            circuits = circuits.Where(c => c.PanelName == panelName).ToList();
        }

        // Apply system type filter
        if (SystemFilterCombo.SelectedItem is ComboBoxItem systemItem && systemItem.Tag is SystemType systemType)
        {
            circuits = circuits.Where(c => c.SystemType == systemType).ToList();
        }

        var displayItems = circuits.Select(c => new CircuitDisplayItem
        {
            Id = c.Id,
            Name = c.GetStandardName(),
            SystemType = c.SystemType.GetDisplayName(),
            TotalLength = c.GetTotalLength(),
            Circuit = c
        }).ToList();

        CircuitListView.ItemsSource = displayItems;
    }

    private void LoadUnassignedSegments()
    {
        var segments = _dataService.GetAllSegments()
            .Where(s => !s.CircuitId.HasValue)
            .Select(s => new SegmentDisplayItem
            {
                Id = s.Id,
                Specification = s.Configuration.GetSpecificationString(),
                Length = s.TotalLength,
                IsVerified = s.IsVerified,
                Segment = s
            }).ToList();

        UnassignedSegmentListView.ItemsSource = segments;
    }

    private void LoadCircuitSegments(Circuit circuit)
    {
        SelectedCircuitText.Text = $"Segments in {circuit.GetStandardName()}";

        var segments = circuit.Segments.Select(s => new SegmentDisplayItem
        {
            Id = s.Id,
            Specification = s.Configuration.GetSpecificationString(),
            Length = s.TotalLength,
            IsVerified = s.IsVerified,
            Segment = s
        }).ToList();

        SegmentListView.ItemsSource = segments;
    }

    private void PanelFilterCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        LoadCircuits();
    }

    private void SystemFilterCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        LoadCircuits();
    }

    private void CircuitListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CircuitListView.SelectedItem is CircuitDisplayItem item)
        {
            LoadCircuitSegments(item.Circuit);
        }
        else
        {
            SelectedCircuitText.Text = "Select a circuit";
            SegmentListView.ItemsSource = null;
        }
    }

    private void NewCircuitButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new NewCircuitDialog(_panelNames);
        if (dialog.ShowDialog() == true)
        {
            _dataService.CreateCircuit(
                dialog.PanelName,
                dialog.CircuitNumber,
                dialog.SystemType);

            LoadCircuits();
        }
    }

    private void DeleteCircuitButton_Click(object sender, RoutedEventArgs e)
    {
        if (CircuitListView.SelectedItem is not CircuitDisplayItem item)
        {
            MessageBox.Show("Please select a circuit to delete.", "Delete Circuit",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"Delete circuit '{item.Name}'?\n\nWire segments will be unassigned but not deleted.",
            "Delete Circuit",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            _dataService.RemoveCircuit(item.Id);
            LoadCircuits();
            LoadUnassignedSegments();
        }
    }

    private void AssignSegmentButton_Click(object sender, RoutedEventArgs e)
    {
        if (CircuitListView.SelectedItem is not CircuitDisplayItem circuitItem)
        {
            MessageBox.Show("Please select a circuit first.", "Assign Segment",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (UnassignedSegmentListView.SelectedItem is not SegmentDisplayItem segmentItem)
        {
            MessageBox.Show("Please select an unassigned segment.", "Assign Segment",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        _dataService.AssignSegmentToCircuit(segmentItem.Id, circuitItem.Id);

        LoadCircuits();
        LoadCircuitSegments(circuitItem.Circuit);
        LoadUnassignedSegments();
    }

    private void UnassignSegmentButton_Click(object sender, RoutedEventArgs e)
    {
        if (SegmentListView.SelectedItem is not SegmentDisplayItem segmentItem)
        {
            MessageBox.Show("Please select a segment to unassign.", "Unassign Segment",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        _dataService.UnassignSegmentFromCircuit(segmentItem.Id);

        LoadCircuits();

        if (CircuitListView.SelectedItem is CircuitDisplayItem circuitItem)
        {
            LoadCircuitSegments(circuitItem.Circuit);
        }

        LoadUnassignedSegments();
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    // Display item classes
    private class CircuitDisplayItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SystemType { get; set; } = string.Empty;
        public double TotalLength { get; set; }
        public Circuit Circuit { get; set; } = null!;
    }

    private class SegmentDisplayItem
    {
        public Guid Id { get; set; }
        public string Specification { get; set; } = string.Empty;
        public double Length { get; set; }
        public bool IsVerified { get; set; }
        public WireSegment Segment { get; set; } = null!;
    }
}
