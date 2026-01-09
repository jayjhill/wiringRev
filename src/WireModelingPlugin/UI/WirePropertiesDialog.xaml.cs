using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WireModelingPlugin.Models;
using WireModelingPlugin.Services;

namespace WireModelingPlugin.UI;

/// <summary>
/// Wire properties configuration dialog.
/// </summary>
public partial class WirePropertiesDialog : Window
{
    private readonly bool _isDefaultSettings;
    private bool _isUpdating;

    public WireConfiguration Configuration { get; private set; }

    public WirePropertiesDialog(WireConfiguration initialConfig, bool isDefaultSettings = false)
    {
        InitializeComponent();

        Configuration = initialConfig.Clone();
        _isDefaultSettings = isDefaultSettings;

        if (_isDefaultSettings)
        {
            Title = "Default Wire Properties";
        }

        InitializeComboBoxes();
        LoadConfiguration();
    }

    private void InitializeComboBoxes()
    {
        // Quick Select - standard configurations
        var standardConfigs = WireConfiguration.GetStandardConfigurations();
        QuickSelectCombo.Items.Add(new ComboBoxItem { Content = "-- Select Standard --" });
        foreach (var config in standardConfigs)
        {
            QuickSelectCombo.Items.Add(new ComboBoxItem
            {
                Content = config.Name,
                Tag = config
            });
        }

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
            NeutralSizeCombo.Items.Add(new ComboBoxItem
            {
                Content = gauge.GetDisplayName(),
                Tag = gauge
            });
        }

        // Add "Same as phase" option for neutral
        NeutralSizeCombo.Items.Insert(0, new ComboBoxItem { Content = "(Same as phase)" });
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
        ConductorCountCombo.SelectedIndex = Configuration.ConductorCount - 2;

        // Set ground size
        foreach (ComboBoxItem item in GroundSizeCombo.Items)
        {
            if (item.Tag is WireGauge gauge && gauge == Configuration.GroundSize)
            {
                GroundSizeCombo.SelectedItem = item;
                break;
            }
        }

        // Set neutral size
        if (Configuration.NeutralSize.HasValue)
        {
            foreach (ComboBoxItem item in NeutralSizeCombo.Items)
            {
                if (item.Tag is WireGauge gauge && gauge == Configuration.NeutralSize.Value)
                {
                    NeutralSizeCombo.SelectedItem = item;
                    break;
                }
            }
        }
        else
        {
            NeutralSizeCombo.SelectedIndex = 0;
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

        // Set insulation color
        if (!string.IsNullOrEmpty(Configuration.InsulationColor))
        {
            foreach (ComboBoxItem item in InsulationColorCombo.Items)
            {
                if (item.Content.ToString() == Configuration.InsulationColor)
                {
                    InsulationColorCombo.SelectedItem = item;
                    break;
                }
            }
        }
        else
        {
            InsulationColorCombo.SelectedIndex = 0;
        }

        _isUpdating = false;
        UpdatePreview();
    }

    private void QuickSelectCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isUpdating || QuickSelectCombo.SelectedIndex <= 0)
            return;

        if (QuickSelectCombo.SelectedItem is ComboBoxItem item && item.Tag is WireConfiguration config)
        {
            Configuration = config.Clone();
            LoadConfiguration();
        }
    }

    private void WireTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isUpdating)
            return;

        if (WireTypeCombo.SelectedItem is ComboBoxItem item && item.Tag is WireInsulationType type)
        {
            Configuration.InsulationType = type;
            UpdatePreview();
        }
    }

    private void GaugeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isUpdating)
            return;

        if (GaugeCombo.SelectedItem is ComboBoxItem item && item.Tag is WireGauge gauge)
        {
            Configuration.Gauge = gauge;
            UpdatePreview();
        }
    }

    private void UpdatePreview()
    {
        // Update configuration from current selections
        if (ConductorCountCombo.SelectedItem is ComboBoxItem countItem)
        {
            Configuration.ConductorCount = int.Parse(countItem.Content.ToString()!);
        }

        if (GroundSizeCombo.SelectedItem is ComboBoxItem groundItem && groundItem.Tag is WireGauge groundGauge)
        {
            Configuration.GroundSize = groundGauge;
        }

        if (NeutralSizeCombo.SelectedIndex == 0)
        {
            Configuration.NeutralSize = null;
        }
        else if (NeutralSizeCombo.SelectedItem is ComboBoxItem neutralItem && neutralItem.Tag is WireGauge neutralGauge)
        {
            Configuration.NeutralSize = neutralGauge;
        }

        Configuration.VoltageRating = VoltageRatingCombo.SelectedIndex == 0 ? VoltageRating.V300 : VoltageRating.V600;

        Configuration.TemperatureRating = TempRatingCombo.SelectedIndex switch
        {
            0 => TemperatureRating.Celsius60,
            1 => TemperatureRating.Celsius75,
            2 => TemperatureRating.Celsius90,
            _ => TemperatureRating.Celsius75
        };

        if (InsulationColorCombo.SelectedItem is ComboBoxItem colorItem)
        {
            var colorStr = colorItem.Content.ToString();
            Configuration.InsulationColor = string.IsNullOrEmpty(colorStr) ? null : colorStr;
        }

        // Update preview text
        SpecPreviewText.Text = Configuration.GetSpecificationString();

        // Update color preview
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
