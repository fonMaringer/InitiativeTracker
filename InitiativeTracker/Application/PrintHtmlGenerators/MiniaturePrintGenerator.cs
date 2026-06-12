using InitiativeTracker.Domain;
using System.Text;
using System.Web;

namespace InitiativeTracker.Application.PrintHtmlGenerators;

public record MiniaturePrintDataDto(
    string Name,
    CreatureSize Size,
    int Quantity,
    string ImageBase64);

public class MiniaturePrintGenerator
{
    const int ColumnsOnPage = 5;

    static readonly Dictionary<CreatureSize, (int WidthMm, int HeightMm)> SizeDimensions = new()
    {
        [CreatureSize.Tiny]       = (16,  13),
        [CreatureSize.Small]      = (32,  25),
        [CreatureSize.Medium]     = (32,  25),
        [CreatureSize.Large]      = (64,  50),
        [CreatureSize.Huge]       = (96,  75),
        [CreatureSize.Gargantuan]  = (128, 100)
    };

    static readonly CreatureSize[] SizeSortOrder = Enum.GetValues<CreatureSize>();

    public string Generate(IEnumerable<MiniaturePrintDataDto> items)
    {
        var grouped = items
            .GroupBy(i => i.Size)
            .OrderBy(g => Array.IndexOf(SizeSortOrder, g.Key))
            .ToList();

        var sb = new StringBuilder();

        WriteHtmlHeader(sb);

        foreach (var group in grouped)
        {
            var dim = SizeDimensions[group.Key];
            WriteSizeGroup(sb, dim.WidthMm, dim.HeightMm, group.ToList());
        }

        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    static void WriteHtmlHeader(StringBuilder sb)
    {
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
        sb.AppendLine("         justify-content:center; gap:1mm; margin-bottom:10mm; }");
        sb.AppendLine(".lbl { width:100%; text-align:center; font-weight:bold; background:#eee; ");
        sb.AppendLine("       padding:3px 6px; border-radius:4px; margin:4px 2px; font-size:11px; }");
        sb.AppendLine(".cell { border:1.25px solid #000; overflow:hidden; display:flex; ");
        sb.AppendLine("        flex-direction:column; break-inside-page:avoid; }");
        sb.AppendLine(".slot { flex:1; overflow:hidden; }");
        sb.AppendLine(".flipped { transform:rotate(180deg); }");
         sb.AppendLine("img   { width:100%; height:100%; object-fit:cover; display:block; }");
        sb.AppendLine("</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
    }

    static void WriteSizeGroup(
        StringBuilder sb,
        int widthMm,
        int heightMm,
        List<MiniaturePrintDataDto> items)
    {
        var imgCells = new List<string>();
        string cellDims = $"width:{widthMm}mm; height:{heightMm}mm;";

        foreach (var item in items)
        {
            string safeName = HttpUtility.HtmlEncode(item.Name ?? "Unnamed");
            sb.AppendLine($"<label class=\"lbl\">{safeName} &times;  {item.Quantity}</label>");

            int cellsNeeded = (item.Quantity + 1) / 2;
            string b64Src = $"data:image/png;base64,{item.ImageBase64}";

            for (int c = 0; c < cellsNeeded; c++)
            {
                var cellSb = new StringBuilder();
                cellSb.Append($"<div class=\"cell\" style=\"{cellDims}\">");
                cellSb.Append($"<div class=\"slot flipped\"><img src=\"{b64Src}\"/></div>");
                cellSb.AppendLine($"<div class=\"slot\"><img src=\"{b64Src}\"/></div>");
                cellSb.AppendLine("</div>");
                imgCells.Add(cellSb.ToString());
            }
        }

        int remainder = imgCells.Count % ColumnsOnPage;
        int padCount = remainder == 0 ? 0 : ColumnsOnPage - remainder;

        int maxW = ColumnsOnPage * widthMm + (ColumnsOnPage - 1);

        sb.Append($"<section class=\"sheet\" style=\"max-width:{maxW}mm;\">");

        foreach (string cellHtml in imgCells)
            sb.Append(cellHtml);

        for (int i = 0; i < padCount; i++)
            sb.Append($"<div class=\"cell\" style=\"{cellDims}\"></div>");

        sb.AppendLine("</section>");
    }
}
