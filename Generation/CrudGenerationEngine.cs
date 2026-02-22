using Tarantuly.Metadata;

namespace Tarantuly.Generation;

public sealed class CrudGenerationEngine
{
    private readonly IReadOnlyList<ICrudTemplate> _templates;

    public CrudGenerationEngine(IEnumerable<ICrudTemplate> templates)
    {
        _templates = templates.ToArray();
    }

    public IReadOnlyList<CrudFileOutput> Generate(CrudModel model, CrudGenerationOptions? options = null)
    {
        options ??= new CrudGenerationOptions();

        var outputs = new List<CrudFileOutput>();
        foreach (var entity in model.Entities)
        {
            var context = new CrudTemplateContext(model, entity, options.RootNamespace, options.UiFlavor);
            foreach (var template in _templates)
            {
                outputs.AddRange(template.Render(context));
            }
        }

        return outputs;
    }
}
