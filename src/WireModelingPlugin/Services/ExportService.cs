using System.Text;
using ClosedXML.Excel;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using WireModelingPlugin.Models;

namespace WireModelingPlugin.Services;

/// <summary>
/// Service for exporting wire data to various formats.
/// </summary>
public class ExportService
{
    /// <summary>
    /// Exports wire pull schedule to Excel format.
    /// </summary>
    public void ExportToExcel(List<Circuit> circuits, string filePath)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Wire Pull Schedule");

        // Header
        worksheet.Cell(1, 1).Value = "Wire Pull Schedule";
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 16;
        worksheet.Range(1, 1, 1, 6).Merge();

        worksheet.Cell(2, 1).Value = $"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}";
        worksheet.Range(2, 1, 2, 6).Merge();

        // Column headers
        int headerRow = 4;
        worksheet.Cell(headerRow, 1).Value = "Circuit";
        worksheet.Cell(headerRow, 2).Value = "Panel";
        worksheet.Cell(headerRow, 3).Value = "Wire Specification";
        worksheet.Cell(headerRow, 4).Value = "System Type";
        worksheet.Cell(headerRow, 5).Value = "Length (ft)";
        worksheet.Cell(headerRow, 6).Value = "Notes";

        var headerRange = worksheet.Range(headerRow, 1, headerRow, 6);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
        headerRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;

        // Data rows
        int row = headerRow + 1;
        double totalLength = 0;

        foreach (var circuit in circuits.OrderBy(c => c.PanelName).ThenBy(c => c.CircuitNumber))
        {
            var config = circuit.GetPrimaryConfiguration();
            var length = circuit.GetTotalLength();
            totalLength += length;

            worksheet.Cell(row, 1).Value = circuit.GetStandardName();
            worksheet.Cell(row, 2).Value = circuit.PanelName;
            worksheet.Cell(row, 3).Value = config?.GetSpecificationString() ?? "N/A";
            worksheet.Cell(row, 4).Value = circuit.SystemType.GetDisplayName();
            worksheet.Cell(row, 5).Value = length;
            worksheet.Cell(row, 6).Value = circuit.Description ?? "";

            row++;
        }

        // Total row
        row++;
        worksheet.Cell(row, 4).Value = "TOTAL:";
        worksheet.Cell(row, 4).Style.Font.Bold = true;
        worksheet.Cell(row, 5).Value = totalLength;
        worksheet.Cell(row, 5).Style.Font.Bold = true;
        worksheet.Cell(row, 5).Style.NumberFormat.Format = "0.0";

        // Format number columns
        worksheet.Column(5).Style.NumberFormat.Format = "0.0";

        // Auto-fit columns
        worksheet.Columns().AdjustToContents();

        workbook.SaveAs(filePath);
    }

    /// <summary>
    /// Exports wire pull schedule to PDF format.
    /// </summary>
    public void ExportToPdf(List<Circuit> circuits, string filePath)
    {
        var document = new PdfDocument();
        document.Info.Title = "Wire Pull Schedule";

        var page = document.AddPage();
        var gfx = XGraphics.FromPdfPage(page);

        var titleFont = new XFont("Arial", 18, XFontStyle.Bold);
        var headerFont = new XFont("Arial", 10, XFontStyle.Bold);
        var normalFont = new XFont("Arial", 9, XFontStyle.Regular);

        double y = 40;
        double leftMargin = 40;

        // Title
        gfx.DrawString("Wire Pull Schedule", titleFont, XBrushes.Black, leftMargin, y);
        y += 25;

        gfx.DrawString($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}", normalFont, XBrushes.Gray, leftMargin, y);
        y += 30;

        // Column positions
        double[] colX = { leftMargin, leftMargin + 80, leftMargin + 150, leftMargin + 300, leftMargin + 380, leftMargin + 450 };
        string[] headers = { "Circuit", "Panel", "Wire Specification", "System", "Length (ft)", "Notes" };

        // Draw headers
        for (int i = 0; i < headers.Length; i++)
        {
            gfx.DrawString(headers[i], headerFont, XBrushes.Black, colX[i], y);
        }
        y += 5;

        // Draw header line
        gfx.DrawLine(XPens.Black, leftMargin, y, page.Width - 40, y);
        y += 15;

        double totalLength = 0;

        foreach (var circuit in circuits.OrderBy(c => c.PanelName).ThenBy(c => c.CircuitNumber))
        {
            // Check for page break
            if (y > page.Height - 60)
            {
                page = document.AddPage();
                gfx = XGraphics.FromPdfPage(page);
                y = 40;

                // Redraw headers
                for (int i = 0; i < headers.Length; i++)
                {
                    gfx.DrawString(headers[i], headerFont, XBrushes.Black, colX[i], y);
                }
                y += 5;
                gfx.DrawLine(XPens.Black, leftMargin, y, page.Width - 40, y);
                y += 15;
            }

            var config = circuit.GetPrimaryConfiguration();
            var length = circuit.GetTotalLength();
            totalLength += length;

            gfx.DrawString(circuit.GetStandardName(), normalFont, XBrushes.Black, colX[0], y);
            gfx.DrawString(circuit.PanelName, normalFont, XBrushes.Black, colX[1], y);
            gfx.DrawString(config?.GetSpecificationString() ?? "N/A", normalFont, XBrushes.Black, colX[2], y);
            gfx.DrawString(circuit.SystemType.GetDisplayName(), normalFont, XBrushes.Black, colX[3], y);
            gfx.DrawString(length.ToString("F1"), normalFont, XBrushes.Black, colX[4], y);
            gfx.DrawString(circuit.Description ?? "", normalFont, XBrushes.Black, colX[5], y);

            y += 15;
        }

        // Total
        y += 10;
        gfx.DrawLine(XPens.Black, leftMargin, y, page.Width - 40, y);
        y += 15;
        gfx.DrawString("TOTAL:", headerFont, XBrushes.Black, colX[3], y);
        gfx.DrawString(totalLength.ToString("F1") + " ft", headerFont, XBrushes.Black, colX[4], y);

        document.Save(filePath);
    }

    /// <summary>
    /// Exports wire pull schedule to CSV format.
    /// </summary>
    public void ExportToCsv(List<Circuit> circuits, string filePath)
    {
        var sb = new StringBuilder();

        // Header
        sb.AppendLine("Circuit,Panel,Wire Specification,System Type,Length (ft),Notes");

        double totalLength = 0;

        foreach (var circuit in circuits.OrderBy(c => c.PanelName).ThenBy(c => c.CircuitNumber))
        {
            var config = circuit.GetPrimaryConfiguration();
            var length = circuit.GetTotalLength();
            totalLength += length;

            sb.AppendLine($"\"{circuit.GetStandardName()}\",\"{circuit.PanelName}\",\"{config?.GetSpecificationString() ?? "N/A"}\",\"{circuit.SystemType.GetDisplayName()}\",{length:F1},\"{circuit.Description ?? ""}\"");
        }

        // Total row
        sb.AppendLine($",,,,{totalLength:F1},TOTAL");

        File.WriteAllText(filePath, sb.ToString());
    }

    /// <summary>
    /// Exports material takeoff to Excel format.
    /// </summary>
    public void ExportMaterialTakeoffToExcel(
        Dictionary<string, double> lengthsByType,
        Dictionary<WireGauge, double> lengthsByGauge,
        string filePath)
    {
        using var workbook = new XLWorkbook();

        // By Type sheet
        var typeSheet = workbook.Worksheets.Add("By Wire Type");
        typeSheet.Cell(1, 1).Value = "Material Takeoff - By Wire Type";
        typeSheet.Cell(1, 1).Style.Font.Bold = true;
        typeSheet.Cell(1, 1).Style.Font.FontSize = 14;

        typeSheet.Cell(3, 1).Value = "Wire Specification";
        typeSheet.Cell(3, 2).Value = "Length (ft)";
        typeSheet.Cell(3, 3).Value = "Length (m)";
        typeSheet.Range(3, 1, 3, 3).Style.Font.Bold = true;
        typeSheet.Range(3, 1, 3, 3).Style.Fill.BackgroundColor = XLColor.LightGray;

        int row = 4;
        double totalLength = 0;
        foreach (var kv in lengthsByType.OrderByDescending(x => x.Value))
        {
            typeSheet.Cell(row, 1).Value = kv.Key;
            typeSheet.Cell(row, 2).Value = kv.Value;
            typeSheet.Cell(row, 3).Value = kv.Value * 0.3048;
            totalLength += kv.Value;
            row++;
        }

        row++;
        typeSheet.Cell(row, 1).Value = "TOTAL";
        typeSheet.Cell(row, 1).Style.Font.Bold = true;
        typeSheet.Cell(row, 2).Value = totalLength;
        typeSheet.Cell(row, 2).Style.Font.Bold = true;
        typeSheet.Cell(row, 3).Value = totalLength * 0.3048;
        typeSheet.Cell(row, 3).Style.Font.Bold = true;

        typeSheet.Column(2).Style.NumberFormat.Format = "0.0";
        typeSheet.Column(3).Style.NumberFormat.Format = "0.0";
        typeSheet.Columns().AdjustToContents();

        // By Gauge sheet
        var gaugeSheet = workbook.Worksheets.Add("By Wire Gauge");
        gaugeSheet.Cell(1, 1).Value = "Material Takeoff - By Wire Gauge";
        gaugeSheet.Cell(1, 1).Style.Font.Bold = true;
        gaugeSheet.Cell(1, 1).Style.Font.FontSize = 14;

        gaugeSheet.Cell(3, 1).Value = "Wire Gauge";
        gaugeSheet.Cell(3, 2).Value = "Length (ft)";
        gaugeSheet.Cell(3, 3).Value = "Length (m)";
        gaugeSheet.Range(3, 1, 3, 3).Style.Font.Bold = true;
        gaugeSheet.Range(3, 1, 3, 3).Style.Fill.BackgroundColor = XLColor.LightGray;

        row = 4;
        foreach (var kv in lengthsByGauge.OrderBy(x => x.Key.GetNumericValue()))
        {
            gaugeSheet.Cell(row, 1).Value = kv.Key.GetDisplayName();
            gaugeSheet.Cell(row, 2).Value = kv.Value;
            gaugeSheet.Cell(row, 3).Value = kv.Value * 0.3048;
            row++;
        }

        gaugeSheet.Column(2).Style.NumberFormat.Format = "0.0";
        gaugeSheet.Column(3).Style.NumberFormat.Format = "0.0";
        gaugeSheet.Columns().AdjustToContents();

        workbook.SaveAs(filePath);
    }

    /// <summary>
    /// Exports material takeoff to PDF format.
    /// </summary>
    public void ExportMaterialTakeoffToPdf(
        Dictionary<string, double> lengthsByType,
        Dictionary<WireGauge, double> lengthsByGauge,
        string filePath)
    {
        var document = new PdfDocument();
        document.Info.Title = "Material Takeoff";

        var page = document.AddPage();
        var gfx = XGraphics.FromPdfPage(page);

        var titleFont = new XFont("Arial", 18, XFontStyle.Bold);
        var sectionFont = new XFont("Arial", 14, XFontStyle.Bold);
        var headerFont = new XFont("Arial", 10, XFontStyle.Bold);
        var normalFont = new XFont("Arial", 9, XFontStyle.Regular);

        double y = 40;
        double leftMargin = 40;

        // Title
        gfx.DrawString("Material Takeoff", titleFont, XBrushes.Black, leftMargin, y);
        y += 25;
        gfx.DrawString($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}", normalFont, XBrushes.Gray, leftMargin, y);
        y += 40;

        // By Wire Type section
        gfx.DrawString("By Wire Type", sectionFont, XBrushes.Black, leftMargin, y);
        y += 25;

        gfx.DrawString("Wire Specification", headerFont, XBrushes.Black, leftMargin, y);
        gfx.DrawString("Length (ft)", headerFont, XBrushes.Black, leftMargin + 250, y);
        gfx.DrawString("Length (m)", headerFont, XBrushes.Black, leftMargin + 340, y);
        y += 5;
        gfx.DrawLine(XPens.Black, leftMargin, y, leftMargin + 420, y);
        y += 15;

        double totalLength = 0;
        foreach (var kv in lengthsByType.OrderByDescending(x => x.Value))
        {
            gfx.DrawString(kv.Key, normalFont, XBrushes.Black, leftMargin, y);
            gfx.DrawString(kv.Value.ToString("F1"), normalFont, XBrushes.Black, leftMargin + 250, y);
            gfx.DrawString((kv.Value * 0.3048).ToString("F1"), normalFont, XBrushes.Black, leftMargin + 340, y);
            totalLength += kv.Value;
            y += 15;
        }

        y += 5;
        gfx.DrawLine(XPens.Black, leftMargin, y, leftMargin + 420, y);
        y += 15;
        gfx.DrawString("TOTAL", headerFont, XBrushes.Black, leftMargin, y);
        gfx.DrawString(totalLength.ToString("F1"), headerFont, XBrushes.Black, leftMargin + 250, y);
        gfx.DrawString((totalLength * 0.3048).ToString("F1"), headerFont, XBrushes.Black, leftMargin + 340, y);

        y += 50;

        // By Gauge section
        gfx.DrawString("By Wire Gauge", sectionFont, XBrushes.Black, leftMargin, y);
        y += 25;

        gfx.DrawString("Wire Gauge", headerFont, XBrushes.Black, leftMargin, y);
        gfx.DrawString("Length (ft)", headerFont, XBrushes.Black, leftMargin + 150, y);
        gfx.DrawString("Length (m)", headerFont, XBrushes.Black, leftMargin + 240, y);
        y += 5;
        gfx.DrawLine(XPens.Black, leftMargin, y, leftMargin + 320, y);
        y += 15;

        foreach (var kv in lengthsByGauge.OrderBy(x => x.Key.GetNumericValue()))
        {
            gfx.DrawString(kv.Key.GetDisplayName(), normalFont, XBrushes.Black, leftMargin, y);
            gfx.DrawString(kv.Value.ToString("F1"), normalFont, XBrushes.Black, leftMargin + 150, y);
            gfx.DrawString((kv.Value * 0.3048).ToString("F1"), normalFont, XBrushes.Black, leftMargin + 240, y);
            y += 15;
        }

        document.Save(filePath);
    }

    /// <summary>
    /// Exports material takeoff to CSV format.
    /// </summary>
    public void ExportMaterialTakeoffToCsv(
        Dictionary<string, double> lengthsByType,
        Dictionary<WireGauge, double> lengthsByGauge,
        string filePath)
    {
        var sb = new StringBuilder();

        sb.AppendLine("MATERIAL TAKEOFF BY WIRE TYPE");
        sb.AppendLine("Wire Specification,Length (ft),Length (m)");

        double totalLength = 0;
        foreach (var kv in lengthsByType.OrderByDescending(x => x.Value))
        {
            sb.AppendLine($"\"{kv.Key}\",{kv.Value:F1},{kv.Value * 0.3048:F1}");
            totalLength += kv.Value;
        }
        sb.AppendLine($"TOTAL,{totalLength:F1},{totalLength * 0.3048:F1}");

        sb.AppendLine();
        sb.AppendLine("MATERIAL TAKEOFF BY WIRE GAUGE");
        sb.AppendLine("Wire Gauge,Length (ft),Length (m)");

        foreach (var kv in lengthsByGauge.OrderBy(x => x.Key.GetNumericValue()))
        {
            sb.AppendLine($"\"{kv.Key.GetDisplayName()}\",{kv.Value:F1},{kv.Value * 0.3048:F1}");
        }

        File.WriteAllText(filePath, sb.ToString());
    }
}
