using System;
using System.IO;
using System.Text;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Kehlet.SourceGenerator;

using SK = SyntaxKind;
using TFO = TargetFilterOptions;

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
    public static IncrementalValuesProvider<T> CreateTargetProvider<T>(
        this IncrementalGeneratorInitializationContext self,
        string fullyQualifiedMetadataName,
        Func<SyntaxNode, CancellationToken, bool> predicate,
        Func<GeneratorAttributeSyntaxContext, CancellationToken, T> transform) =>
        self.SyntaxProvider.ForAttributeWithMetadataName(fullyQualifiedMetadataName, predicate, transform);

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
    /// <param name="options"></param>
    /// <param name="predicate">A function that determines if the given <see cref="SyntaxNode"/> attribute target (<see
    /// cref="GeneratorAttributeSyntaxContext.TargetNode"/>) should be transformed.  Nodes that do not pass this
    /// predicate will not have their attributes looked at at all.</param>
    /// <param name="transform">A function that performs the transform. This will only be passed nodes that return <see
    /// langword="true"/> for <paramref name="predicate"/> and which have a matching <see cref="AttributeData"/> whose
    /// <see cref="AttributeData.AttributeClass"/> has the same fully qualified, metadata name as <paramref
    /// name="fullyQualifiedMetadataName"/>.</param>
    public static IncrementalValuesProvider<T> CreateTargetProvider<T>(
        this IncrementalGeneratorInitializationContext self,
        string fullyQualifiedMetadataName,
        TFO options,
        Func<SyntaxNode, CancellationToken, bool> predicate,
        Func<GeneratorAttributeSyntaxContext, CancellationToken, T> transform)
    {
        static bool Predicate(SyntaxNode node, TFO options, Func<SyntaxNode, CancellationToken, bool> filter, CancellationToken ct)
        {
            // Check user predicate first.
            if (filter(node, ct) is false)
            {
                return false;
            }

            if (options is TFO.None)
            {
                // No options, so target is valid. 
                return true;
            }

            if (node is not MemberDeclarationSyntax member)
            {
                // Node cannot be partial.
                return false;
            }

            if (options.HasFlag(TFO.Partial) && member.Modifiers.Any(SK.PartialKeyword) is false)
            {
                return false;
            }

            if (options.HasFlag(TFO.Static) && member.Modifiers.Any(SK.StaticKeyword) is false)
            {
                return false;
            }

            return true;
        }

        return self.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName,
            (node, token) => Predicate(node, options, predicate, token),
            transform
        );
    }

    public static Unit AddSource<T>(this IncrementalGeneratorPostInitializationContext context, string text)
    {
        context.AddSource(typeof(T).FullName! + ".g.cs", SourceText.From(text, Encoding.UTF8));
        return unit;
    }

    public static Unit AddSource(this IncrementalGeneratorPostInitializationContext context, string hintName, string text)
    {
        context.AddSource(hintName, SourceText.From(text, Encoding.UTF8));
        return unit;
    }

    public static Unit AddSource<T>(this IncrementalGeneratorPostInitializationContext context, Stream stream)
    {
        context.AddSource(typeof(T).FullName! + ".g.cs", SourceText.From(stream, Encoding.UTF8));
        return unit;
    }

    /// <summary>
    /// Helper for context.RegisterSourceOutput(source, (productionContext, tuple) => action(productionContext, tuple.Item1, tuple.Item2));
    /// </summary>
    /// <param name="context"></param>
    /// <param name="source"></param>
    /// <param name="action"></param>
    /// <typeparam name="TItem1"></typeparam>
    /// <typeparam name="TItem2"></typeparam>
    /// <returns></returns>
    public static Unit RegisterSourceOutput<TItem1, TItem2>(
        this IncrementalGeneratorInitializationContext context,
        IncrementalValuesProvider<(TItem1, TItem2)> source,
        Action<SourceProductionContext, TItem1, TItem2> action)
    {
        context.RegisterSourceOutput(source, (productionContext, tuple) => action(productionContext, tuple.Item1, tuple.Item2));
        return unit;
    }

    /// <summary>
    /// Helper for context.RegisterSourceOutput(source, (productionContext, tuple) => action(productionContext, tuple.Item1, tuple.Item2, tuple.Item3));
    /// </summary>
    /// <param name="context"></param>
    /// <param name="source"></param>
    /// <param name="action"></param>
    /// <typeparam name="TItem1"></typeparam>
    /// <typeparam name="TItem2"></typeparam>
    /// <typeparam name="TItem3"></typeparam>
    /// <returns></returns>
    public static Unit RegisterSourceOutput<TItem1, TItem2, TItem3>(
        this IncrementalGeneratorInitializationContext context,
        IncrementalValuesProvider<(TItem1, TItem2, TItem3)> source,
        Action<SourceProductionContext, TItem1, TItem2, TItem3> action)
    {
        context.RegisterSourceOutput(source, (productionContext, tuple) => action(productionContext, tuple.Item1, tuple.Item2, tuple.Item3));
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

internal static class TargetFilterOptionsExtensions
{
    public static bool HasFlag(this TFO self, TFO flag) => (self & flag) == flag;
}