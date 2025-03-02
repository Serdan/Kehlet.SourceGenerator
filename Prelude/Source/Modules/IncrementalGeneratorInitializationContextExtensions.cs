using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Kehlet.SourceGenerator;

internal static class IncrementalGeneratorInitializationContextExtensions
{
    /// <summary>
    /// Creates an <see cref="IncrementalValuesProvider{T}"/> that can provide a transform over all <see
    /// cref="SyntaxNode"/>s if that node has an attribute on it that binds to a <see cref="INamedTypeSymbol"/> with the
    /// same fully-qualified metadata as the provided <paramref name="fullyQualifiedMetadataName"/>. <paramref
    /// name="fullyQualifiedMetadataName"/> should be the fully-qualified, metadata name of the attribute, including the
    /// <c>Attribute</c> suffix.  For example <c>"System.CLSCompliantAttribute"</c> for <see
    /// cref="System.CLSCompliantAttribute"/>.
    /// </summary>
    /// <param name="self"></param>
    /// <param name="fullyQualifiedMetadataName"></param>
    /// <param name="predicate">A function that determines if the given <see cref="SyntaxNode"/> attribute target (<see
    /// cref="GeneratorAttributeSyntaxContext.TargetNode"/>) should be transformed.  Nodes that do not pass this
    /// predicate will not have their attributes looked at at all.</param>
    /// <param name="transform">A function that performs the transform. This will only be passed nodes that return <see
    /// langword="true"/> for <paramref name="predicate"/> and which have a matching <see cref="AttributeData"/> whose
    /// <see cref="AttributeData.AttributeClass"/> has the same fully qualified, metadata name as <paramref
    /// name="fullyQualifiedMetadataName"/>.</param>
    public static IncrementalValuesProvider<T> SyntaxForAttribute<T>(
        this IncrementalGeneratorInitializationContext self,
        string fullyQualifiedMetadataName,
        Func<SyntaxNode, CancellationToken, bool> predicate,
        Func<GeneratorAttributeSyntaxContext, CancellationToken, T> transform) =>
        self.SyntaxProvider.ForAttributeWithMetadataName(fullyQualifiedMetadataName, predicate, transform);

    public static Unit RegisterType(this IncrementalGeneratorInitializationContext self, string hintName, string text)
    {
        self.RegisterPostInitializationOutput(ctx => ctx.AddSource(hintName, SourceText.From(text, Encoding.UTF8)));
        return unit;
    }
    public static Unit RegisterType<T>(this IncrementalGeneratorInitializationContext self, string source)
    {
        self.RegisterPostInitializationOutput(ctx => ctx.AddSource(typeof(T).FullName + ".g.cs", SourceText.From(source, Encoding.UTF8)));
        return unit;
    }
}
