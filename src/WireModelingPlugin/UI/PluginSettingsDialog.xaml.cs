using System.Windows;
using System.Windows.Controls;
using WireModelingPlugin.Models;
using WireModelingPlugin.Services;

namespace WireModelingPlugin.UI;

/// <summary>
/// Dialog for configuring plugin settings.
/// </summary>
public partial class PluginSettingsDialog : Window
{
    private readonly PluginSettings _settings;

    public PluginSettingsDialog(PluginSettings settings)
    {
        InitializeComponent();

        _settings = settings;
        LoadSettings();
    }

    private void LoadSettings()
    {
        // Length calculation
        WastePercentageText.Text = _settings.LengthCalculation.WastePercentage.ToString("F1");
        JunctionBoxAllowanceText.Text = _settings.LengthCalculation.JunctionBoxAllowance.ToString("F2");
        PanelTerminationText.Text = _settings.LengthCalculation.PanelTerminationAllowance.ToString("F2");
        DeviceTerminationText.Text = _settings.LengthCalculation.DeviceTerminationAllowance.ToString("F2");
        RoundUpCheckBox.IsChecked = _settings.LengthCalculation.RoundUpToNearestFoot;

        // Drawing
        ShowPreviewCheckBox.IsChecked = _settings.ShowDrawingPreview;
        AutoSnapCheckBox.IsChecked = _settings.AutoSnapToConnectors;
        SnapToleranceText.Text = _settings.SnapTolerance.ToString("F2");
        AutoAssignCheckBox.IsChecked = _settings.AutoAssignToCircuit;

        // Export
        DefaultFormatCombo.SelectedIndex = _settings.DefaultExportFormat switch
        {
            ExportFormat.PDF => 1,
            ExportFormat.CSV => 2,
            _ => 0
        };

        LengthUnitCombo.SelectedIndex = _settings.DefaultLengthUnit switch
        {
            LengthUnit.Inches => 1,
            LengthUnit.Meters => 2,
            _ => 0
        };
    }

    private bool SaveSettings()
    {
        // Validate and save length calculation settings
        if (!double.TryParse(WastePercentageText.Text, out var wastePercentage) || wastePercentage < 0)
        {
            MessageBox.Show("Invalid waste percentage. Please enter a number >= 0.", "Validation Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            WastePercentageText.Focus();
            return false;
        }

        if (!double.TryParse(JunctionBoxAllowanceText.Text, out var junctionBox) || junctionBox < 0)
        {
            MessageBox.Show("Invalid junction box allowance. Please enter a number >= 0.", "Validation Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            JunctionBoxAllowanceText.Focus();
            return false;
        }

        if (!double.TryParse(PanelTerminationText.Text, out var panelTerm) || panelTerm < 0)
        {
            MessageBox.Show("Invalid panel termination allowance. Please enter a number >= 0.", "Validation Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            PanelTerminationText.Focus();
            return false;
        }

        if (!double.TryParse(DeviceTerminationText.Text, out var deviceTerm) || deviceTerm < 0)
        {
            MessageBox.Show("Invalid device termination allowance. Please enter a number >= 0.", "Validation Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            DeviceTerminationText.Focus();
            return false;
        }

        if (!double.TryParse(SnapToleranceText.Text, out var snapTolerance) || snapTolerance < 0)
        {
            MessageBox.Show("Invalid snap tolerance. Please enter a number >= 0.", "Validation Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            SnapToleranceText.Focus();
            return false;
        }

        // Apply settings
        _settings.LengthCalculation.WastePercentage = wastePercentage;
        _settings.LengthCalculation.JunctionBoxAllowance = junctionBox;
        _settings.LengthCalculation.PanelTerminationAllowance = panelTerm;
        _settings.LengthCalculation.DeviceTerminationAllowance = deviceTerm;
        _settings.LengthCalculation.RoundUpToNearestFoot = RoundUpCheckBox.IsChecked ?? true;

        _settings.ShowDrawingPreview = ShowPreviewCheckBox.IsChecked ?? true;
        _settings.AutoSnapToConnectors = AutoSnapCheckBox.IsChecked ?? true;
        _settings.SnapTolerance = snapTolerance;
        _settings.AutoAssignToCircuit = AutoAssignCheckBox.IsChecked ?? true;

        _settings.DefaultExportFormat = DefaultFormatCombo.SelectedIndex switch
        {
            1 => ExportFormat.PDF,
            2 => ExportFormat.CSV,
            _ => ExportFormat.Excel
        };

        _settings.DefaultLengthUnit = LengthUnitCombo.SelectedIndex switch
        {
            1 => LengthUnit.Inches,
            2 => LengthUnit.Meters,
            _ => LengthUnit.Feet
        };

        return true;
    }

    private void ResetButton_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "Reset all settings to defaults?",
            "Reset Settings",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            _settings.ResetToDefaults();
            LoadSettings();
        }
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        if (SaveSettings())
        {
            DialogResult = true;
            Close();
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
