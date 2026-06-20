using System.Text;
using System.Web;

namespace InitiativeTracker.Application.PrintHtmlGenerators;

public record PokerCardPrintDataDto
(
    string Title,
    string? Subtitle,
    IReadOnlyCollection<string> Flags,
    IReadOnlyCollection<string> AdditionalInfo,
    string Content,
    string? Subfooter,
    string? Footer
);

public class PokerCardPrintGenerator
{
    public string Generate(IEnumerable<PokerCardPrintDataDto> items)
    {
        var sb = new StringBuilder();

        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"en\">");
        sb.AppendLine("<head>");
        sb.AppendLine("  <meta charset=\"utf-8\" />");
        sb.AppendLine("  <title>InitiativeTracker - Cards Print</title>");
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
        sb.AppendLine(".flag-badge { display:inline-block; background:#c0392b; color:#fff; ");
        sb.AppendLine("                     font-size:8px; padding:1px 4px; border-radius:3px; margin-right:4px; }");
        sb.AppendLine(".card-additional-info { font-size:8px; margin-bottom:2px; flex-shrink:0; padding-left: 4px; ");
        sb.AppendLine("              background-color: white; }");
        sb.AppendLine(".additional-info-badge { display:inline-block; ");
        sb.AppendLine("                     font-size:8px; padding:1px 4px; border-radius:3px; margin-right:4px; }");
        sb.AppendLine(".card-content { flex:1; overflow:hidden; font-size:8px; ");
        sb.AppendLine("                     background-color: white; padding: 3px; }");
        sb.AppendLine(".card-subfooter { font-size:8px; margin-top:2px; flex-shrink:0; padding: 3px; ");
        sb.AppendLine("              background-color: white; }");
        sb.AppendLine(".card-footer { font-weight:bold; font-size:8px; text-align:center; ");
        sb.AppendLine("                margin-top:2px; flex-shrink:0; background-color: white; }");
        sb.AppendLine(".card-content img { max-width:100%; height:auto; }");
        sb.AppendLine("a { color: inherit; text-decoration: inherit; }");
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

    private static void WritePokerCard(StringBuilder sb, PokerCardPrintDataDto item)
    {
        sb.Append($"<div class=\"poker-card\">");
        sb.Append($"  <div class=\"card-title\">{HttpUtility.HtmlEncode(item.Title)}</div>");
        sb.Append($"  <div class=\"card-subtitle\">");
        foreach (var flag in item.Flags)
        {
            sb.Append($"<span class=\"flag-badge\">{flag}</span>");
        }
        if (item.Subtitle is not null)
        {
            sb.Append($"{HttpUtility.HtmlEncode(item.Subtitle)}");
        }
        sb.AppendLine("</div>");
        if (item.AdditionalInfo.Any())
        {
            sb.Append($"  <div class=\"card-additional-info\">");
            foreach (var info in item.AdditionalInfo)
            {
                sb.Append($"<span class=\"additional-info-badge\">{info}</span>");
            }
            sb.AppendLine("</div>");
        }
        sb.AppendLine($"  <div class=\"card-content\">{item.Content}</div>");
        if (item.Subfooter is not null)
        {
            sb.AppendLine($"  <div class=\"card-subfooter\">{item.Subfooter}</div>");
        }
        if (item.Footer is not null)
        {
            sb.AppendLine($"  <div class=\"card-footer\">{item.Footer}</div>");
        }
        sb.AppendLine("</div>");
    }
}