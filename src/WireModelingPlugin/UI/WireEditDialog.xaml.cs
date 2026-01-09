using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Autodesk.Revit.DB;
using WireModelingPlugin.Models;
using WireModelingPlugin.Services;

namespace WireModelingPlugin.UI;

/// <summary>
/// Dialog for editing existing wire segments.
/// </summary>
public partial class WireEditDialog : Window
{
    private readonly Element _element;
    private bool _isUpdating;

    public WireConfiguration Configuration { get; private set; }

    public WireEditDialog(Element element, WireConfiguration initialConfig)
    {
        InitializeComponent();

        _element = element;
        Configuration = initialConfig.Clone();

        ElementIdText.Text = $"Element ID: {element.Id.Value}";

        InitializeComboBoxes();
        LoadConfiguration();
    }

    private void InitializeComboBoxes()
    {
        // Wire Types
        foreach (WireInsulationType type in Enum.GetValues<WireInsulationType>())
        {
            WireTypeCombo.Items.Add(new ComboBoxItem
            {
                Content = type.GetDisplayName(),
                Tag = type
            });
        }

        // Wire Gauges
        foreach (WireGauge gauge in Enum.GetValues<WireGauge>())
        {
            GaugeCombo.Items.Add(new ComboBoxItem
            {
                Content = gauge.GetDisplayName(),
                Tag = gauge
            });
            GroundSizeCombo.Items.Add(new ComboBoxItem
            {
                Content = gauge.GetDisplayName(),
                Tag = gauge
            });
        }
    }

    private void LoadConfiguration()
    {
        _isUpdating = true;

        // Set wire type
        foreach (ComboBoxItem item in WireTypeCombo.Items)
        {
            if (item.Tag is WireInsulationType type && type == Configuration.InsulationType)
            {
                WireTypeCombo.SelectedItem = item;
                break;
            }
        }

        // Set gauge
        foreach (ComboBoxItem item in GaugeCombo.Items)
        {
            if (item.Tag is WireGauge gauge && gauge == Configuration.Gauge)
            {
                GaugeCombo.SelectedItem = item;
                break;
            }
        }

        // Set conductor count
        ConductorCountCombo.SelectedIndex = Math.Max(0, Configuration.ConductorCount - 2);

        // Set ground size
        foreach (ComboBoxItem item in GroundSizeCombo.Items)
        {
            if (item.Tag is WireGauge gauge && gauge == Configuration.GroundSize)
            {
                GroundSizeCombo.SelectedItem = item;
                break;
            }
        }

        // Set voltage rating
        VoltageRatingCombo.SelectedIndex = Configuration.VoltageRating == VoltageRating.V300 ? 0 : 1;

        // Set temperature rating
        TempRatingCombo.SelectedIndex = Configuration.TemperatureRating switch
        {
            TemperatureRating.Celsius60 => 0,
            TemperatureRating.Celsius75 => 1,
            TemperatureRating.Celsius90 => 2,
            _ => 1
        };

        _isUpdating = false;
        UpdatePreview();
    }

    private void UpdatePreview()
    {
        if (_isUpdating)
            return;

        // Update configuration from current selections
        if (WireTypeCombo.SelectedItem is ComboBoxItem typeItem && typeItem.Tag is WireInsulationType type)
        {
            Configuration.InsulationType = type;
        }

        if (GaugeCombo.SelectedItem is ComboBoxItem gaugeItem && gaugeItem.Tag is WireGauge gauge)
        {
            Configuration.Gauge = gauge;
        }

        if (ConductorCountCombo.SelectedItem is ComboBoxItem countItem)
        {
            Configuration.ConductorCount = int.Parse(countItem.Content.ToString()!);
        }

        if (GroundSizeCombo.SelectedItem is ComboBoxItem groundItem && groundItem.Tag is WireGauge groundGauge)
        {
            Configuration.GroundSize = groundGauge;
        }

        Configuration.VoltageRating = VoltageRatingCombo.SelectedIndex == 0 ? VoltageRating.V300 : VoltageRating.V600;

        Configuration.TemperatureRating = TempRatingCombo.SelectedIndex switch
        {
            0 => TemperatureRating.Celsius60,
            1 => TemperatureRating.Celsius75,
            2 => TemperatureRating.Celsius90,
            _ => TemperatureRating.Celsius75
        };

        // Update preview
        SpecText.Text = Configuration.GetSpecificationString();
        var color = WireColorService.GetDisplayColor(Configuration);
        ColorPreview.Fill = new SolidColorBrush(color);
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        UpdatePreview();
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
