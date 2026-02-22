using System.Text;
using Tarantuly.Generation;
using Tarantuly.Metadata;

namespace Tarantuly.Templates.MudBlazor;

public sealed class MudBlazorListTemplate : ICrudTemplate
{
    public string Name => "MudBlazorList";

    public IReadOnlyList<CrudFileOutput> Render(CrudTemplateContext context)
    {
        var model = context.Entity;
        var keyType = ResolveKeyType(model);
        var componentPath = $"{model.Name}/{model.Name}List.razor";

        var builder = new StringBuilder();
        builder.AppendLine("@using MudBlazor");
        builder.AppendLine("@using Tarantuly.Abstractions");
        builder.AppendLine($"@inject ICrudDataService<{model.FullName}, {keyType}> DataService");
        builder.AppendLine($"<MudText Typo=\"Typo.h5\">{model.DisplayName}</MudText>");
        builder.AppendLine($"<MudTable T=\"{model.FullName}\" Items=\"@Items\" Dense=\"true\" Hover=\"true\">");
        builder.AppendLine("    <HeaderContent>");
        foreach (var property in model.Properties.Where(IsDisplayable))
        {
            builder.AppendLine($"        <MudTh>{property.Name}</MudTh>");
        }

        builder.AppendLine("        <MudTh>Actions</MudTh>");
        builder.AppendLine("    </HeaderContent>");
        builder.AppendLine("    <RowTemplate>");
        foreach (var property in model.Properties.Where(IsDisplayable))
        {
            builder.AppendLine($"        <MudTd DataLabel=\"{property.Name}\">@context.{property.Name}</MudTd>");
        }

        builder.AppendLine("        <MudTd>");
        builder.AppendLine("            <MudButton Variant=\"Variant.Text\" Size=\"Size.Small\" OnClick=\"() => OnDetails.InvokeAsync(context)\">Details</MudButton>");
        builder.AppendLine("            <MudButton Variant=\"Variant.Text\" Size=\"Size.Small\" Color=\"Color.Primary\" OnClick=\"() => OnEdit.InvokeAsync(context)\">Edit</MudButton>");
        builder.AppendLine("            <MudButton Variant=\"Variant.Text\" Size=\"Size.Small\" Color=\"Color.Error\" OnClick=\"() => DeleteAsync(context)\">Delete</MudButton>");
        builder.AppendLine("        </MudTd>");
        builder.AppendLine("    </RowTemplate>");
        builder.AppendLine("</MudTable>");
        builder.AppendLine("@code {");
        builder.AppendLine($"    private List<{model.FullName}> Items = [];");
        builder.AppendLine($"    [Parameter] public EventCallback<{model.FullName}> OnDetails {{ get; set; }}");
        builder.AppendLine($"    [Parameter] public EventCallback<{model.FullName}> OnEdit {{ get; set; }}");
        builder.AppendLine("    protected override async Task OnInitializedAsync()");
        builder.AppendLine("    {");
        builder.AppendLine("        Items = (await DataService.ListAsync()).ToList();");
        builder.AppendLine("    }");
        builder.AppendLine($"    private async Task DeleteAsync({model.FullName} item)");
        builder.AppendLine("    {");
        builder.AppendLine($"        await DataService.SoftDeleteAsync(item.{ResolveKeyPropertyName(model)});");
        builder.AppendLine("        Items = (await DataService.ListAsync()).ToList();");
        builder.AppendLine("    }");
        builder.AppendLine("}");

        return [new CrudFileOutput(componentPath, builder.ToString())];
    }

    private static bool IsDisplayable(CrudPropertyModel property)
    {
        return property.Kind is not CrudPropertyKind.NavigationCollection and not CrudPropertyKind.NavigationReference;
    }

    private static string ResolveKeyType(CrudEntityModel entity)
    {
        return entity.Properties.FirstOrDefault(property => property.Kind == CrudPropertyKind.Key)?.TypeName ?? "int";
    }

    private static string ResolveKeyPropertyName(CrudEntityModel entity)
    {
        return entity.Properties.FirstOrDefault(property => property.Kind == CrudPropertyKind.Key)?.Name ?? "Id";
    }
}
