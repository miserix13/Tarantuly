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
        var keyType = ResolveKeyType(entity);
        if (!entity.Properties.Any())
        {
            return [];
        }

        var path = $"{entity.Name}/{entity.Name}Details.razor";

        var builder = new StringBuilder();
        builder.AppendLine("@using MudBlazor");
        builder.AppendLine("@using Tarantuly.Abstractions");
        builder.AppendLine($"@inject ICrudDataService<{entity.FullName}, {keyType}> DataService");
        builder.AppendLine($"<MudText Typo=\"Typo.h5\">{entity.DisplayName} Details</MudText>");
        builder.AppendLine("<MudPaper Class=\"pa-4\" Elevation=\"0\">");
        builder.AppendLine("@if (Model is null)");
        builder.AppendLine("{");
        builder.AppendLine("  <MudText Typo=\"Typo.body2\">Loading...</MudText>");
        builder.AppendLine("}");
        builder.AppendLine("else");
        builder.AppendLine("{");

        foreach (var property in entity.Properties.Where(IsDisplayable))
        {
            builder.AppendLine("  <MudStack Row=\"true\" Class=\"mb-2\">");
            builder.AppendLine($"    <MudText Typo=\"Typo.subtitle2\" Class=\"me-2\">{property.Name}:</MudText>");
            builder.AppendLine($"    <MudText Typo=\"Typo.body2\">@Model.{property.Name}</MudText>");
            builder.AppendLine("  </MudStack>");
        }

        builder.AppendLine("}");

        builder.AppendLine("</MudPaper>");
        builder.AppendLine("@code {");
        builder.AppendLine($"  [Parameter] public {keyType} Id {{ get; set; }} = default!;");
        builder.AppendLine($"  private {entity.FullName}? Model {{ get; set; }}");
        builder.AppendLine("  protected override async Task OnParametersSetAsync()");
        builder.AppendLine("  {");
        builder.AppendLine("    Model = await DataService.GetByIdAsync(Id);");
        builder.AppendLine("  }");
        builder.AppendLine("}");

        return [new CrudFileOutput(path, builder.ToString())];
    }

    private static bool IsDisplayable(CrudPropertyModel property)
    {
        return property.Kind is not CrudPropertyKind.NavigationCollection and not CrudPropertyKind.NavigationReference;
    }

    private static string ResolveKeyType(CrudEntityModel entity)
    {
        return entity.Properties.FirstOrDefault(property => property.Kind == CrudPropertyKind.Key)?.TypeName ?? "int";
    }
}
