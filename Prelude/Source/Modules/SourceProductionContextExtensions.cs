using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Kehlet.SourceGenerator;

internal static class SourceProductionContextExtensions
{
    public static SourceProductionContext AddSource(this SourceProductionContext context, string hintName, string source)
    {
        context.AddSource(hintName, source);
        return context;
    }

    public static SourceProductionContext AddSource(this SourceProductionContext context, string hintName, SourceText source)
    {
        context.AddSource(hintName, source);
        return context;
    }
}