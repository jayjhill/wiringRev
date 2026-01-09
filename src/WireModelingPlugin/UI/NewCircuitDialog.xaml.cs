using System.Windows;
using System.Windows.Controls;
using WireModelingPlugin.Models;

namespace WireModelingPlugin.UI;

/// <summary>
/// Dialog for creating a new circuit.
/// </summary>
public partial class NewCircuitDialog : Window
{
    public string PanelName { get; private set; } = string.Empty;
    public string CircuitNumber { get; private set; } = string.Empty;
    public SystemType SystemType { get; private set; } = SystemType.Power;
    public string? Description { get; private set; }

    public NewCircuitDialog(List<string> existingPanels)
    {
        InitializeComponent();

        // Load existing panels
        foreach (var panel in existingPanels)
        {
            PanelCombo.Items.Add(panel);
        }

        // Load system types
        foreach (SystemType type in Enum.GetValues<SystemType>())
        {
            SystemTypeCombo.Items.Add(new ComboBoxItem
            {
                Content = type.GetDisplayName(),
                Tag = type
            });
        }
        SystemTypeCombo.SelectedIndex = 0;
    }

    private void CreateButton_Click(object sender, RoutedEventArgs e)
    {
        // Validate
        var panelName = PanelCombo.Text?.Trim();
        var circuitNumber = CircuitNumberText.Text?.Trim();

        if (string.IsNullOrEmpty(panelName))
        {
            MessageBox.Show("Please enter a panel name.", "Validation Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            PanelCombo.Focus();
            return;
        }

        if (string.IsNullOrEmpty(circuitNumber))
        {
            MessageBox.Show("Please enter a circuit number.", "Validation Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            CircuitNumberText.Focus();
            return;
        }

        PanelName = panelName;
        CircuitNumber = circuitNumber;

        if (SystemTypeCombo.SelectedItem is ComboBoxItem item && item.Tag is SystemType type)
        {
            SystemType = type;
        }

        var description = DescriptionText.Text?.Trim();
        Description = string.IsNullOrEmpty(description) ? null : description;

        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
