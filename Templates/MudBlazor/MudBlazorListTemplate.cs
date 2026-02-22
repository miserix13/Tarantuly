using System.Text;
using Tarantuly.Generation;

namespace Tarantuly.Templates.MudBlazor;

public sealed class MudBlazorListTemplate : ICrudTemplate
{
    public string Name => "MudBlazorList";

    public IReadOnlyList<CrudFileOutput> Render(CrudTemplateContext context)
    {
        var model = context.Entity;
        var componentPath = $"{model.Name}/{model.Name}List.razor";

        var builder = new StringBuilder();
        builder.AppendLine("@using MudBlazor");
        builder.AppendLine($"<MudText Typo=\"Typo.h5\">{model.DisplayName}</MudText>");
        builder.AppendLine("<MudTable Items=\"@Items\" Dense=\"true\" Hover=\"true\"> ");
        builder.AppendLine("    <HeaderContent>");
        foreach (var property in model.Properties.Where(p => p.Kind is not Tarantuly.Metadata.CrudPropertyKind.NavigationCollection and not Tarantuly.Metadata.CrudPropertyKind.NavigationReference))
        {
            builder.AppendLine($"        <MudTh>{property.Name}</MudTh>");
        }

        builder.AppendLine("    </HeaderContent>");
        builder.AppendLine("</MudTable>");
        builder.AppendLine("@code {");
        builder.AppendLine("    private readonly List<object> Items = []; ");
        builder.AppendLine("}");

        return [new CrudFileOutput(componentPath, builder.ToString())];
    }
}
