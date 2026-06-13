using System.Text;
using System.Web;
using InitiativeTracker.Domain.Enums;

namespace InitiativeTracker.Application.PrintHtmlGenerators;

public record ItemPrintDataDto(
    string Name,
    ItemRarity Rarity,
    bool RequiresAttunement,
    string DescriptionHtml);

public class ItemPrintGenerator
{
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
        sb.AppendLine(".sheet { display:flex; flex-wrap:wrap; justify-content:center; gap:0mm; ");
        sb.AppendLine("          margin-bottom:10mm; margin-top:10mm }");
        sb.AppendLine(".poker-card { width:2.5in; height:3.5in; border-radius:8px; padding: 5px; ");
        sb.AppendLine("              border:2px solid #000; display:flex; flex-direction:column; ");
        sb.AppendLine("              break-inside:avoid; overflow:hidden; background-color: #ccc; }");
        sb.AppendLine(".poker-card > div:last-child { border-radius: 0 0 4px 4px; }");
        sb.AppendLine(".card-title { font-weight:bold; font-size:12px; margin-bottom:2px; ");
        sb.AppendLine("              background-color: white; border-radius: 4px 4px 0 0; padding-left: 3px; }");
        sb.AppendLine(".card-subtitle { font-size:8px; margin-bottom:2px; flex-shrink:0; padding-left: 4px; ");
        sb.AppendLine("              background-color: white; }");
        sb.AppendLine(".card-components { font-size:10px; color:#444; margin-bottom:4px; flex-shrink:0; }");
        sb.AppendLine(".component-badge { display:inline-block; background:#2980b9; color:#fff; ");
        sb.AppendLine("                     font-size:8px; padding:1px 4px; border-radius:3px; margin-right:4px; }");
        sb.AppendLine(".attunement-badge { display:inline-block; background:#c0392b; color:#fff; ");
        sb.AppendLine("                     font-size:8px; padding:1px 4px; border-radius:3px; margin-right:4px; }");
        sb.AppendLine(".card-content { flex:1; overflow:hidden; font-size:8px; ");
        sb.AppendLine("                     background-color: white; padding: 3px; }");
        sb.AppendLine(".card-footer { font-weight:bold; font-size:12px; text-align:center; ");
        sb.AppendLine("                margin-top:4px; padding-top:3px; border-top:1px solid #ccc; flex-shrink:0; }");
        sb.AppendLine(".card-content img { max-width:100%; height:auto; }");
        sb.AppendLine("</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");

        sb.Append($"<section class=\"sheet\">");

        foreach (var item in items)
            WritePokerCard(sb, item);

        sb.AppendLine("</section>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    static void WritePokerCard(StringBuilder sb, ItemPrintDataDto item)
    {
        string safeName = HttpUtility.HtmlEncode(item.Name);
        string rarityLabel = item.Rarity.ToString();
        bool hasAttunement = item.RequiresAttunement;
        string description = item.DescriptionHtml ?? "";

        sb.Append($"<div class=\"poker-card\">");
        sb.Append($"  <div class=\"card-title\">{safeName}</div>");
        sb.Append($"  <div class=\"card-subtitle\">");
        if (hasAttunement)
            sb.Append($"<span class=\"attunement-badge\">ATT</span>");
        sb.Append($"{HttpUtility.HtmlEncode(rarityLabel)}");
        sb.AppendLine("</div>");
        sb.AppendLine($"  <div class=\"card-content\">{description}</div>");
        sb.AppendLine("</div>");
    }
}
