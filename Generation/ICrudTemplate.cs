namespace Tarantuly.Generation;

public interface ICrudTemplate
{
    string Name { get; }

    IReadOnlyList<CrudFileOutput> Render(CrudTemplateContext context);
}
