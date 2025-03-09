using System;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Kehlet.SourceGenerator;

internal static class IncrementalGeneratorInitializationContextExtensions
{
    /// <summary>
    /// Alias for IncrementalGeneratorInitializationContext.SyntaxProvider.ForAttributeWithMetadataName
    /// <para/>
    /// Creates an <see cref="IncrementalValuesProvider{T}"/> that can provide a transform over all <see
    /// cref="SyntaxNode"/>s if that node has an attribute on it that binds to a <see cref="INamedTypeSymbol"/> with the
    /// same fully-qualified metadata as the provided <paramref name="fullyQualifiedMetadataName"/>. <paramref
    /// name="fullyQualifiedMetadataName"/> should be the fully-qualified, metadata name of the attribute, including the
    /// <c>Attribute</c> suffix.  For example <c>"System.CLSCompliantAttribute"</c> for <see
    /// cref="System.CLSCompliantAttribute"/>.
    /// </summary>
    /// <param name="self"></param>
    /// <param name="fullyQualifiedMetadataName">Use the value returned by typeof(T).FullName</param>
    /// <param name="predicate">A function that determines if the given <see cref="SyntaxNode"/> attribute target (<see
    /// cref="GeneratorAttributeSyntaxContext.TargetNode"/>) should be transformed.  Nodes that do not pass this
    /// predicate will not have their attributes looked at at all.</param>
    /// <param name="transform">A function that performs the transform. This will only be passed nodes that return <see
    /// langword="true"/> for <paramref name="predicate"/> and which have a matching <see cref="AttributeData"/> whose
    /// <see cref="AttributeData.AttributeClass"/> has the same fully qualified, metadata name as <paramref
    /// name="fullyQualifiedMetadataName"/>.</param>
    public static IncrementalValuesProvider<T> GetTargetProvider<T>(
        this IncrementalGeneratorInitializationContext self,
        string fullyQualifiedMetadataName,
        Func<SyntaxNode, CancellationToken, bool> predicate,
        Func<GeneratorAttributeSyntaxContext, CancellationToken, T> transform) =>
        self.SyntaxProvider.ForAttributeWithMetadataName(fullyQualifiedMetadataName, predicate, transform);

    public static IncrementalValuesProvider<T> GetTargetProvider<T>(
        this IncrementalGeneratorInitializationContext self,
        string fullyQualifiedMetadataName,
        TargetFilterOptions options,
        Func<SyntaxNode, CancellationToken, bool> predicate,
        Func<GeneratorAttributeSyntaxContext, CancellationToken, T> transform)
    {
        static bool Predicate(SyntaxNode node, TargetFilterOptions options, Func<SyntaxNode, CancellationToken, bool> filter, CancellationToken ct)
        {
            // Check user predicate first.
            if (filter(node, ct) is false)
            {
                return false;
            }

            if (options is TargetFilterOptions.None)
            {
                // No options, so target is valid. 
                return true;
            }

            if (node is not MemberDeclarationSyntax member)
            {
                // Node cannot be partial.
                return false;
            }

            if ((options & TargetFilterOptions.Partial) is TargetFilterOptions.Partial)
            {
                if (member.Modifiers.Any(SyntaxKind.PartialKeyword) is false)
                {
                    return false;
                }
            }

            if ((options & TargetFilterOptions.Static) is TargetFilterOptions.Static)
            {
                if (member.Modifiers.Any(SyntaxKind.StaticKeyword) is false)
                {
                    return false;
                }
            }

            return true;
        }

        return self.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName,
            (node, token) => Predicate(node, options, predicate, token),
            transform
        );
    }

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

[Flags]
internal enum TargetFilterOptions
{
    None = 0,
    Partial = 1,
    Static = 2
}
