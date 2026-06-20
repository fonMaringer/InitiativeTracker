using System.Text;
using InitiativeTracker.Domain.Enums;

namespace InitiativeTracker.Application.PrintHtmlGenerators;

public record MiniaturePrintDataDto(
    string Name,
    CreatureSize Size,
    int Quantity,
    string ImageBase64);

public class MiniaturePrintGenerator
{
    private const int BaseWidth = 25;
    private const int BaseHeight = 32;
    
    private static readonly Dictionary<CreatureSize, (int WidthMm, int HeightMm)> SizeDimensions = new()
    {
        [CreatureSize.Tiny]       = (BaseWidth / 2, BaseHeight / 2),
        [CreatureSize.Small]      = (BaseWidth, BaseHeight),
        [CreatureSize.Medium]     = (BaseWidth, BaseHeight),
        [CreatureSize.Large]      = (BaseWidth * 2, BaseHeight * 2),
        [CreatureSize.Huge]       = (BaseWidth * 3, BaseHeight * 3),
        [CreatureSize.Gargantuan]  = (BaseWidth * 4, BaseHeight * 4),
    };

    private static readonly CreatureSize[] SizeSortOrder = Enum.GetValues<CreatureSize>();

    public string Generate(IEnumerable<MiniaturePrintDataDto> items)
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
        sb.AppendLine("  <title>InitiativeTracker - Miniature Print</title>");
        sb.AppendLine("  <style>");
        sb.AppendLine("* { margin:0; padding:0; box-sizing:border-box; }");
        sb.AppendLine("@page { size:A4 portrait; margin:10mm; }");
        sb.AppendLine("body { font-family:Arial,sans-serif; font-size:12px; }");
        sb.AppendLine(".sheet { break-inside-page:avoid; display:flex; flex-wrap:wrap; ");
        sb.AppendLine("         justify-content:center; margin-top:5mm; margin-bottom:5mm; }");
        sb.AppendLine(".cell { overflow:hidden; display:flex; ");
        sb.AppendLine("        flex-direction:column; page-break-inside:avoid; }");
        sb.AppendLine(".slot { flex:1; overflow:hidden; border: 0.5px solid #000; }");
        sb.AppendLine(".flipped { transform: rotateX(180deg); }");
        sb.AppendLine("img   { width:100%; height:100%; object-fit:cover; display:block; }");
        sb.AppendLine("</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");

        foreach (var group in grouped)
        {
            var dim = SizeDimensions[group.Key];
            WriteSizeGroup(sb, dim.WidthMm, dim.HeightMm, group.ToList());
        }

        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    private static void WriteSizeGroup(
        StringBuilder sb,
        int widthMm,
        int heightMm,
        List<MiniaturePrintDataDto> items)
    {
        var imgCells = new List<string>();
        var cellDims = $"width:{widthMm}mm; height:{heightMm * 2}mm;";

        foreach (var item in items)
        {
            var b64Src = $"data:image/png;base64,{item.ImageBase64}";

            for (var c = 0; c < item.Quantity; c++)
            {
                var cellSb = new StringBuilder();
                cellSb.Append($"<div class=\"cell\" style=\"{cellDims}\">");
                cellSb.Append($"<div class=\"slot flipped\"><img src=\"{b64Src}\"/></div>");
                cellSb.AppendLine($"<div class=\"slot\"><img src=\"{b64Src}\"/></div>");
                cellSb.AppendLine("</div>");
                imgCells.Add(cellSb.ToString());
            }
        }

        sb.Append($"<section class=\"sheet\">");

        foreach (var cellHtml in imgCells)
            sb.Append(cellHtml);

        sb.AppendLine("</section>");
    }
}
