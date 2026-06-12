using InitiativeTracker.Domain;
using System.Text;
using System.Web;
using InitiativeTracker.Domain.Enums;

namespace InitiativeTracker.Application.PrintHtmlGenerators;

public record SpellPrintDataDto(
    string Name,
    bool VerbalComponent,
    bool SomaticComponent,
    bool MaterialComponent,
    SpellClass SpellClass,
    string DescriptionHtml);

public class SpellPrintGenerator
{
    const int ColumnsOnPage = 4;

    public string Generate(IEnumerable<SpellPrintDataDto> spells)
    {
        var sb = new StringBuilder();

        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"en\">");
        sb.AppendLine("<head>");
        sb.AppendLine("  <meta charset=\"utf-8\" />");
        sb.AppendLine("  <title>InitiativeTracker - Spell Cards Print</title>");
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
        sb.AppendLine(".card-components { font-size:10px; color:#555; margin-bottom:4px; flex-shrink:0; }");
        sb.AppendLine(".component-badge { display:inline-block; background:#2980b9; color:#fff; ");
        sb.AppendLine("                     font-size:9px; padding:1px 4px; border-radius:3px; margin-left:4px; }");
        sb.AppendLine(".card-content { flex:1; overflow:hidden; font-size:11px; line-height:1.35; }");
        sb.AppendLine(".card-footer { font-weight:bold; font-size:12px; text-align:center; ");
        sb.AppendLine("                margin-top:4px; padding-top:3px; border-top:1px solid #ccc; flex-shrink:0; }");
        sb.AppendLine("</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");

        int count = 0;
        var list = spells.ToList();
        count = list.Count;

        int paddedCount = PaddingCountForItems(count);
        sb.Append($"<section class=\"sheet\">");

        foreach (var spell in list)
            WriteSpellCard(sb, spell);

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

    static void WriteSpellCard(StringBuilder sb, SpellPrintDataDto spell)
    {
        string safeName = HttpUtility.HtmlEncode(spell.Name);
        string safeClass = HttpUtility.HtmlEncode(spell.SpellClass.ToString());
        string description = spell.DescriptionHtml ?? "";

        var componentParts = new List<string>();
        if (spell.VerbalComponent) componentParts.Add("V");
        if (spell.SomaticComponent) componentParts.Add("J");
        if (spell.MaterialComponent) componentParts.Add("R");

        sb.Append($"<div class=\"poker-card\">");
        sb.Append($"  <div class=\"card-title\">{safeName}</div>");
        sb.Append($"  <div class=\"card-components\">");
        foreach (var comp in componentParts)
            sb.Append($"<span class=\"component-badge\">{comp}</span>");
        sb.AppendLine("</div>");
        sb.AppendLine($"  <div class=\"card-content\">{description}</div>");
        sb.AppendLine($"  <div class=\"card-footer\">{safeClass}</div>");
        sb.AppendLine("</div>");
    }
}
