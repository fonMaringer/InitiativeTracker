using System.Text;
using InitiativeTracker.Domain.Enums;

namespace InitiativeTracker.Application.PrintHtmlGenerators;

public record StandPrintDataDto(
    CreatureSize Size,
    int Quantity,
    string ImageBase64,
    bool InverseTextColor,
    bool AddIndex,
    int StartIndex);

public class StandPrintGenerator
{
    private static readonly CreatureSize[] SizeSortOrder = Enum.GetValues<CreatureSize>();

    public static readonly Dictionary<CreatureSize, int> StandHeights = new()
    {
        [CreatureSize.Tiny]       = 5,
        [CreatureSize.Small]      = 5,
        [CreatureSize.Medium]     = 5,
        [CreatureSize.Large]      = 10,
        [CreatureSize.Huge]       = 15,
        [CreatureSize.Gargantuan]  = 20,
    };
    
    public string Generate(IEnumerable<StandPrintDataDto> items)
    {
        var grouped = items
            .GroupBy(i => i.Size)
            .OrderBy(g => Array.IndexOf(SizeSortOrder, g.Key))
            .ToList();

        var sb = new StringBuilder();

        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"en\">");
        sb.AppendLine("<head>");
        sb.AppendLine("  <meta charset=\"utf-8\" />");
        sb.AppendLine("  <title>InitiativeTracker - Stand Print</title>");
        sb.AppendLine("  <style>");
        sb.AppendLine("* { margin:0; padding:0; box-sizing:border-box; }");
        sb.AppendLine("@page { size:A4 portrait; margin:10mm; }");
        sb.AppendLine("body { font-family:Arial,sans-serif; font-size:12px; }");
        sb.AppendLine(".sheet { break-inside-page:avoid; display:flex; flex-wrap:wrap; ");
        sb.AppendLine("         justify-content:center; margin-top:5mm; margin-bottom:5mm; }");
        sb.AppendLine(".cell { overflow:hidden; display:flex; ");
        sb.AppendLine("        flex-direction:column; page-break-inside:avoid; }");
        sb.AppendLine(".slot { border: 1px solid #000; box-sizing:border-box; flex-direction: row; display: flex; }");
        sb.AppendLine(".stand-left { width: 50%; height: 100%; border: 0; border-right: 1px solid #000; }");
        sb.AppendLine(".stand-right { width: 50%; height: 100%; border: 0; border-left: 1px solid #000; }");
        sb.AppendLine("</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");

        foreach (var group in grouped)
        {
            var dim = Constants.SizeDimensions[group.Key];
            var h = StandHeights[group.Key];
            WriteSizeGroup(sb, dim.WidthMm, h, group.ToList());
        }

        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    private static void WriteSizeGroup(
        StringBuilder sb,
        int dimMm,
        int standHeightMm,
        List<StandPrintDataDto> items)
    {
        var imgCells = new List<string>();
        var cellDims = $"width:{dimMm}mm; height:{dimMm * 2 + standHeightMm * 2}mm;";

        var halfDim = dimMm / 2;

        foreach (var item in items)
        {
            var b64Src = $"data:image/png;base64,{item.ImageBase64}";
            var bgStyle = $"background-image:url({b64Src}); background-repeat:repeat;";
            var textColor = item.InverseTextColor ? "color: #fff" : "";

            var currentIndex = item.StartIndex;
            for (var c = 0; c < item.Quantity; c++)
            {
                var cellSb = new StringBuilder();
                cellSb.AppendLine($"<div class=\"cell\" style=\"{cellDims} {bgStyle}\">");

                if (item.AddIndex)
                    cellSb.AppendLine($"<div class=\"slot\" style=\"height:{halfDim}mm; align-items: center; padding-left: 5%;\">" +
                        $"<span style=\"font-weight:bold; writing-mode: vertical-lr; position: relative; {textColor}\">{currentIndex}</span></div>");
                else
                    cellSb.AppendLine($"<div class=\"slot\" style=\"height:{halfDim}mm;\"></div>");

                cellSb.AppendLine($"<div class=\"slot\" style=\"height:{standHeightMm}mm;\"><div class=\"stand-left\"></div><div class=\"stand-right\"></div></div>");
                cellSb.AppendLine($"<div class=\"slot\" style=\"height:{standHeightMm}mm;\"><div class=\"stand-left\"></div><div class=\"stand-right\"></div></div>");
                cellSb.AppendLine($"<div class=\"slot\" style=\"height:{halfDim}mm; display:flex; align-items:center; justify-content:flex-end; padding-right: 5%;\">" +
                    $"<span style=\"transform:rotate(-90deg); transform-origin:center center; font-size: large; {textColor}\">&#9786;</span></div>");
                cellSb.AppendLine($"<div class=\"slot\" style=\"height:{dimMm}mm;\"></div>");
                cellSb.AppendLine("</div>");

                imgCells.Add(cellSb.ToString());
                currentIndex++;
            }
        }

        sb.AppendLine("<section class=\"sheet\">");

        foreach (var cellHtml in imgCells)
            sb.AppendLine(cellHtml);

        sb.AppendLine("</section>");
    }
}
