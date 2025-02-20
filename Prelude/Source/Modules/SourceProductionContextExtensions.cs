using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Kehlet.SourceGenerator;

internal static class SourceProductionContextExtensions
{
    public static Unit AddSourceUTF8(this SourceProductionContext context, string hintName, string source)
    {
        context.AddSource(hintName, SourceText.From(source, Encoding.UTF8));
        return unit;
    }

    public static Unit AddSourceUTF8(this SourceProductionContext context, string hintName, IEmitter emitter)
    {
        context.AddSource(hintName, SourceText.From(emitter.ToString(), Encoding.UTF8));
        return unit;
    }
}
