# Revit Wire Modeling Plugin

A custom Autodesk Revit plugin that enables accurate 3D modeling of electrical wire and cable in building projects with full parametric data, automatic length calculations, and integration with Revit's electrical systems.

## Features

### Wire Placement
- **Multi-view placement** — Place wire segments in floor plan, section, elevation, and 3D views
- **Point-to-point drawing** — Click to define wire path vertices with real-time preview
- **Multi-segment runs** — Create continuous wire runs with multiple direction changes
- **Vertical routing** — Model wire runs between floor levels
- **Snap to connectors** — Automatic alignment to electrical device connectors

### Wire Properties
| Property | Description |
|----------|-------------|
| Wire Type | THHN, THWN, XHHW, USE, NM-B |
| Gauge (AWG) | 20 through 4/0 AWG |
| Conductor Count | Number of current-carrying conductors |
| Ground Size | Equipment grounding conductor size |
| Neutral Size | Optional separate neutral size |
| Voltage Rating | 300V or 600V |
| Temperature Rating | 60°C, 75°C, or 90°C |
| Insulation Color | Phase identification |

### Wire Color Coding
Automatic visual color coding based on NEC standards:

**Single Conductor:**
| Color | Gauge | Typical Use |
|-------|-------|-------------|
| White | 14 AWG | 15-amp circuits |
| Yellow | 12 AWG | 20-amp circuits |
| Orange | 10 AWG | 30-amp circuits |
| Black | 8/6 AWG | 40-50 amp circuits |

**Multi-Conductor (NM-B):**
| Color | Configuration |
|-------|---------------|
| Blue | 14/3 NM-B |
| Purple | 12/3 NM-B |
| Pink | 10/3 NM-B |

### Circuit Organization
- Assign wire segments to specific circuits
- Link circuits to distribution panels
- Standard naming convention (Panel-Circuit#)
- System categorization (Lighting, Receptacle, Mechanical, Fire Alarm, Low Voltage)
- Multi-wire circuit support

### Length Calculation
- Automatic segment length calculation
- Circuit total length summation
- Configurable waste/slack percentage
- Junction box allowances
- Panel termination allowances
- Device termination allowances

### Reporting
- Native Revit schedule integration
- Wire pull schedule export (PDF, Excel, CSV)
- Material takeoff by wire type and gauge

## Requirements

- **Autodesk Revit 2026** or later
- **.NET 8.0** or later
- **Windows 10/11** 64-bit

### Why Revit 2026?

This plugin leverages Revit 2026's overhauled Electrical Conductor and Cable Settings system:
- Native cable type definitions
- Multi-core cable support
- Enhanced circuit path routing
- Improved panel schedule organization
- Better API support for electrical workflows

## Installation

### From Release
1. Download the latest release from the [Releases](../../releases) page
2. Run the installer (`.msi` or `.exe`)
3. Restart Revit

### Manual Installation
1. Build the solution (see [Building from Source](#building-from-source))
2. Copy the following files to `%AppData%\Autodesk\Revit\Addins\2026\`:
   - `WireModelingPlugin.dll`
   - `WireModelingPlugin.addin`
3. Restart Revit

## Usage

### Getting Started

1. Open a Revit project with electrical elements
2. Navigate to the **Wire Modeling** tab in the ribbon

### Drawing Wire

1. Click **Draw Wire** in the Draw panel
2. Configure wire properties in the dialog (or use defaults)
3. Click points in the view to define the wire path
4. Press **Escape** or right-click to finish the segment

### Managing Circuits

1. Click **Circuit Manager** in the Properties panel
2. Create new circuits with panel and circuit number
3. Assign wire segments to circuits
4. View total lengths per circuit

### Generating Reports

1. Click **Wire Schedule** or **Material Takeoff** in the Reports panel
2. Preview the report data
3. Export to PDF, Excel, or CSV

### Configuring Settings

1. Click **Plugin Settings** in the Settings panel
2. Adjust length calculation allowances:
   - Waste percentage (default: 10%)
   - Junction box allowance (default: 1 ft)
   - Panel termination (default: 3 ft)
   - Device termination (default: 0.5 ft)
3. Configure drawing preferences

## Project Structure

```
WireModelingPlugin/
├── WireModelingPlugin.sln
└── src/WireModelingPlugin/
    ├── WireModelingPlugin.csproj
    ├── WireModelingPlugin.addin
    ├── WireModelingApplication.cs      # Plugin entry point
    ├── Commands/                        # Ribbon commands
    │   ├── DrawWireCommand.cs
    │   ├── EditWireCommand.cs
    │   ├── WirePropertiesCommand.cs
    │   ├── CircuitManagerCommand.cs
    │   ├── WireScheduleCommand.cs
    │   ├── MaterialTakeoffCommand.cs
    │   └── PluginSettingsCommand.cs
    ├── Models/                          # Data models
    │   ├── WireConfiguration.cs
    │   ├── WireSegment.cs
    │   ├── Circuit.cs
    │   ├── WireGauge.cs
    │   ├── WireInsulationType.cs
    │   ├── SystemType.cs
    │   └── LengthCalculationSettings.cs
    ├── Services/                        # Business logic
    │   ├── WireDataService.cs
    │   ├── WireColorService.cs
    │   ├── ExportService.cs
    │   └── PluginSettings.cs
    ├── UI/                              # WPF dialogs
    │   ├── WirePropertiesDialog.xaml
    │   ├── CircuitManagerDialog.xaml
    │   ├── WireScheduleDialog.xaml
    │   ├── MaterialTakeoffDialog.xaml
    │   └── PluginSettingsDialog.xaml
    └── Utils/                           # Utilities
        ├── ConnectorUtils.cs
        └── RevitParameterUtils.cs
```

## Building from Source

### Prerequisites
- Visual Studio 2022 or later
- .NET 8.0 SDK
- Autodesk Revit 2026 installed

### Build Steps

1. Clone the repository:
   ```bash
   git clone https://github.com/your-org/wiringRev.git
   cd wiringRev
   ```

2. Open `WireModelingPlugin.sln` in Visual Studio

3. Update Revit API references in `WireModelingPlugin.csproj`:
   ```xml
   <Reference Include="RevitAPI">
     <HintPath>C:\Program Files\Autodesk\Revit 2026\RevitAPI.dll</HintPath>
   </Reference>
   <Reference Include="RevitAPIUI">
     <HintPath>C:\Program Files\Autodesk\Revit 2026\RevitAPIUI.dll</HintPath>
   </Reference>
   ```

4. Build the solution:
   ```bash
   dotnet build --configuration Release
   ```

5. Copy output to Revit addins folder:
   ```bash
   copy bin\Release\net8.0\WireModelingPlugin.dll "%AppData%\Autodesk\Revit\Addins\2026\"
   copy src\WireModelingPlugin\WireModelingPlugin.addin "%AppData%\Autodesk\Revit\Addins\2026\"
   ```

## Ribbon Layout

```
[Wire Modeling Tab]
├── Draw Panel
│   ├── Draw Wire      — Create new wire segments
│   └── Edit Wire      — Modify existing wire paths
├── Properties Panel
│   ├── Wire Properties — Set default wire specifications
│   └── Circuit Manager — Organize circuits and assignments
├── Reports Panel
│   ├── Wire Schedule   — Export wire pull schedules
│   └── Material Takeoff — Export quantity summaries
└── Settings Panel
    └── Plugin Settings — Configure calculation factors
```

## Wire Type Reference

| Type | Full Name |
|------|-----------|
| THHN | Thermoplastic High Heat-resistant Nylon-coated |
| THWN | Thermoplastic Heat and Water-resistant Nylon-coated |
| XHHW | Cross-linked polyethylene High Heat-resistant Water-resistant |
| USE | Underground Service Entrance |
| NM-B | Non-Metallic Sheathed Cable (Romex) |

## Out of Scope

The following features are explicitly not included:
- Clash detection
- Electrical load calculations
- Voltage drop calculations
- Conduit modeling
- Automated routing / auto-routing algorithms

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Autodesk Revit API documentation
- NEC (National Electrical Code) for wire sizing standards
