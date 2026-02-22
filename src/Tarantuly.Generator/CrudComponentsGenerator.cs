using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Tarantuly.Generator;

[Generator]
public sealed class CrudComponentsGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static postInitContext =>
        {
            var source = new StringBuilder();
            source.AppendLine("namespace Tarantuly.Generated;");
            source.AppendLine();
            source.AppendLine("public static class CrudGeneratorInfo");
            source.AppendLine("{");
            source.AppendLine("    public const string Version = \"0.1.0\";");
            source.AppendLine("    public const string Message = \"Tarantuly generator bootstrap is active.\";");
            source.AppendLine("}");

            postInitContext.AddSource("CrudGeneratorInfo.g.cs", SourceText.From(source.ToString(), Encoding.UTF8));
        });
    }
}
