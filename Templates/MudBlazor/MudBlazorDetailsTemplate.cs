using System.Text;
using Tarantuly.Generation;
using Tarantuly.Metadata;

namespace Tarantuly.Templates.MudBlazor;

public sealed class MudBlazorDetailsTemplate : ICrudTemplate
{
    public string Name => "MudBlazorDetails";

    public IReadOnlyList<CrudFileOutput> Render(CrudTemplateContext context)
    {
        var entity = context.Entity;
        if (!entity.Properties.Any())
        {
            return [];
        }

        var path = $"{entity.Name}/{entity.Name}Details.razor";

        var builder = new StringBuilder();
        builder.AppendLine("@using MudBlazor");
        builder.AppendLine($"<MudText Typo=\"Typo.h5\">{entity.DisplayName} Details</MudText>");
        builder.AppendLine("<MudPaper Class=\"pa-4\" Elevation=\"0\">");

        foreach (var property in entity.Properties.Where(IsDisplayable))
        {
            builder.AppendLine("  <MudStack Row=\"true\" Class=\"mb-2\">");
            builder.AppendLine($"    <MudText Typo=\"Typo.subtitle2\" Class=\"me-2\">{property.Name}:</MudText>");
            builder.AppendLine($"    <MudText Typo=\"Typo.body2\">@Model.{property.Name}</MudText>");
            builder.AppendLine("  </MudStack>");
        }

        builder.AppendLine("</MudPaper>");
        builder.AppendLine("@code {");
        builder.AppendLine($"  [Parameter] public {entity.FullName} Model {{ get; set; }} = default!;");
        builder.AppendLine("}");

        return [new CrudFileOutput(path, builder.ToString())];
    }

    private static bool IsDisplayable(CrudPropertyModel property)
    {
        return property.Kind is not CrudPropertyKind.NavigationCollection and not CrudPropertyKind.NavigationReference;
    }
}
