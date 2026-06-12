using InitiativeTracker.Domain;
using System.Text;
using System.Web;

namespace InitiativeTracker.Application.PrintHtmlGenerators;

public record ItemPrintDataDto(
    string Name,
    ItemRarity Rarity,
    bool RequiresAttunement,
    string DescriptionHtml);

public class ItemPrintGenerator
{
    const int ColumnsOnPage = 4;

    public string Generate(IEnumerable<ItemPrintDataDto> items)
    {
        var sb = new StringBuilder();

        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"en\">");
        sb.AppendLine("<head>");
        sb.AppendLine("  <meta charset=\"utf-8\" />");
        sb.AppendLine("  <title>InitiativeTracker - Item Cards Print</title>");
        sb.AppendLine("  <style>");
        sb.AppendLine("* { margin:0; padding:0; box-sizing:border-box; }");
        sb.AppendLine("@page { size:A4 portrait; margin:10mm; }");
        sb.AppendLine("body { font-family:Arial,sans-serif; font-size:12px; }");
        sb.AppendLine(".sheet { display:flex; flex-wrap:wrap; justify-content:center; gap:3mm; ");
        sb.AppendLine("          margin-bottom:10mm; }");
        sb.AppendLine(".poker-card { width:2.5in; height:3.5in; border-radius:8px; padding:6px 8px; ");
        sb.AppendLine("              border:1.5px solid #000; display:flex; flex-direction:column; ");
        sb.AppendLine("              break-inside:avoid; overflow:hidden; }");
        sb.AppendLine(".card-title { font-weight:bold; font-size:14px; margin-bottom:3px; line-height:1.2; }");
        sb.AppendLine(".card-subtitle { font-size:10px; color:#555; margin-bottom:4px; flex-shrink:0; }");
        sb.AppendLine(".attunement-badge { display:inline-block; background:#c0392b; color:#fff; ");
        sb.AppendLine("                     font-size:9px; padding:1px 4px; border-radius:3px; margin-left:4px; }");
        sb.AppendLine(".card-content { flex:1; overflow:hidden; font-size:11px; line-height:1.35; }");
        sb.AppendLine(".card-content img { max-width:100%; height:auto; }");
        sb.AppendLine("</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");

        int paddedCount = PaddingCountForItems(items.Count());
        sb.Append($"<section class=\"sheet\">");

        foreach (var item in items)
            WritePokerCard(sb, item);

        for (int i = 0; i < paddedCount; i++)
            sb.AppendLine("<div class=\"poker-card\"></div>");

        sb.AppendLine("</section>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    int PaddingCountForItems(int count)
    {
        int remainder = count % ColumnsOnPage;
        return remainder == 0 ? 0 : ColumnsOnPage - remainder;
    }

    static void WritePokerCard(StringBuilder sb, ItemPrintDataDto item)
    {
        string safeName = HttpUtility.HtmlEncode(item.Name);
        string rarityLabel = item.Rarity.ToString();
        bool hasAttunement = item.RequiresAttunement;
        string description = item.DescriptionHtml ?? "";

        sb.Append($"<div class=\"poker-card\">");
        sb.Append($"  <div class=\"card-title\">{safeName}</div>");
        sb.Append($"  <div class=\"card-subtitle\">{HttpUtility.HtmlEncode(rarityLabel)}");
        if (hasAttunement)
            sb.Append($"<span class=\"attunement-badge\">ATT</span>");
        sb.AppendLine("</div>");
        sb.AppendLine($"  <div class=\"card-content\">{description}</div>");
        sb.AppendLine("</div>");
    }
}
