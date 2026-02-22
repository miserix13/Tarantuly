using System.Text;
using Tarantuly.Generation;
using Tarantuly.Metadata;

namespace Tarantuly.Templates.MudBlazor;

public sealed class MudBlazorUpsertTemplate : ICrudTemplate
{
    public string Name => "MudBlazorUpsert";

    public IReadOnlyList<CrudFileOutput> Render(CrudTemplateContext context)
    {
        var entity = context.Entity;
        var editableProperties = entity.Properties.Where(IsEditable).ToArray();
        var keyType = ResolveKeyType(entity);
        if (editableProperties.Length == 0)
        {
            return [];
        }

        var path = $"{entity.Name}/{entity.Name}Upsert.razor";

        var builder = new StringBuilder();
        builder.AppendLine("@using MudBlazor");
        builder.AppendLine("@using Tarantuly.Abstractions");
        builder.AppendLine($"@inject ICrudDataService<{entity.FullName}, {keyType}> DataService");
        builder.AppendLine("<MudForm @ref=\"_form\">");
        builder.AppendLine("  <MudStack Spacing=\"2\">");

        foreach (var property in editableProperties)
        {
            if (property.TypeName is "System.String" or "string")
            {
                builder.AppendLine($"    <MudTextField T=\"string\" Label=\"{property.Name}\" @bind-Value=\"Model.{property.Name}\" Required=\"{property.IsRequired.ToString().ToLowerInvariant()}\" />");
                continue;
            }

            if (property.TypeName is "System.Int32" or "int" or "System.Int64" or "long" or "System.Decimal" or "decimal" or "System.Double" or "double")
            {
                builder.AppendLine($"    <MudNumericField T=\"{ToNumericType(property.TypeName)}\" Label=\"{property.Name}\" @bind-Value=\"Model.{property.Name}\" Required=\"{property.IsRequired.ToString().ToLowerInvariant()}\" />");
                continue;
            }

            if (property.TypeName is "System.DateTime")
            {
                builder.AppendLine($"    <MudDatePicker Label=\"{property.Name}\" @bind-Date=\"Model.{property.Name}\" />");
                continue;
            }

            builder.AppendLine($"    <MudTextField T=\"string\" Label=\"{property.Name}\" Value=\"@Convert.ToString(Model.{property.Name})\" Disabled=\"true\" />");
        }

        builder.AppendLine("    <MudButton Variant=\"Variant.Filled\" Color=\"Color.Primary\" OnClick=\"SaveAsync\">Save</MudButton>");
        builder.AppendLine("  </MudStack>");
        builder.AppendLine("</MudForm>");
        builder.AppendLine("@code {");
        builder.AppendLine("  private MudForm? _form;");
        builder.AppendLine($"  [Parameter] public {entity.FullName} Model {{ get; set; }} = default!;");
        builder.AppendLine("  [Parameter] public bool IsEditMode { get; set; }");
        builder.AppendLine($"  [Parameter] public EventCallback<{entity.FullName}> OnSaved {{ get; set; }}");
        builder.AppendLine("  private async Task SaveAsync()");
        builder.AppendLine("  {");
        builder.AppendLine("    if (_form is not null)");
        builder.AppendLine("    {");
        builder.AppendLine("      await _form.Validate();");
        builder.AppendLine("      if (!_form.IsValid)");
        builder.AppendLine("      {");
        builder.AppendLine("        return;");
        builder.AppendLine("      }");
        builder.AppendLine("    }");
        builder.AppendLine("    var saved = IsEditMode");
        builder.AppendLine("      ? await DataService.UpdateAsync(Model)");
        builder.AppendLine("      : await DataService.CreateAsync(Model);");
        builder.AppendLine("    await OnSaved.InvokeAsync(saved);");
        builder.AppendLine("  }");
        builder.AppendLine("}");

        return [new CrudFileOutput(path, builder.ToString())];
    }

    private static bool IsEditable(CrudPropertyModel property)
    {
        return property.IsEditable
               && property.Kind is not CrudPropertyKind.NavigationCollection
               && property.Kind is not CrudPropertyKind.NavigationReference
               && property.Kind is not CrudPropertyKind.ConcurrencyToken;
    }

    private static string ToNumericType(string typeName)
    {
        return typeName switch
        {
            "System.Int32" or "int" => "int",
            "System.Int64" or "long" => "long",
            "System.Decimal" or "decimal" => "decimal",
            "System.Double" or "double" => "double",
            _ => "decimal",
        };
    }

    private static string ResolveKeyType(CrudEntityModel entity)
    {
        return entity.Properties.FirstOrDefault(property => property.Kind == CrudPropertyKind.Key)?.TypeName ?? "int";
    }
}
