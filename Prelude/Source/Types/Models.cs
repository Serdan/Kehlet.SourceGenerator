#nullable enable
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Kehlet.SourceGenerator;

/// <summary>
/// Cacheable base data for a C# type.
/// </summary>
internal record TypeBaseData
{
    public required string Modifiers { get; init; }

    public required string Keyword { get; init; }

    public required string Identifier { get; init; }

    public required string TypeParameters { get; init; }

    public required int Arity { get; init; }

    public virtual string GetTypeDeclaration() => $"{Modifiers} {Keyword} {Identifier}{TypeParameters}";

    public static TypeBaseData From(TypeDeclarationSyntax syntax) =>
        new()
        {
            Modifiers = syntax.Modifiers.ToString(),
            Keyword = syntax.GetKeyword(),
            Identifier = syntax.Identifier.ValueText,
            TypeParameters = syntax.TypeParameterList?.ToString() ?? "",
            Arity = syntax.Arity
        };
}

/// <summary>
/// Cacheable data for a C# type. This is sufficient to create a valid partial declaration for unnested types.
/// </summary>
internal record TypeData : TypeBaseData
{
    public required string Namespace { get; init; }

    public virtual string GetNamespaceDeclaration() => string.IsNullOrWhiteSpace(Namespace) ? "" : $"namespace {Namespace};";

    public virtual string GetFileName(bool fullyQualified = false)
    {
        var name = "";
        if (fullyQualified && string.IsNullOrWhiteSpace(Namespace) is false)
        {
            name += Namespace + ".";
        }

        name += Identifier;
        if (Arity > 0)
        {
            name += $"`{Arity}";
        }

        return name + ".g.cs";
    }

    public static TypeData From(INamedTypeSymbol symbol, TypeDeclarationSyntax syntax) =>
        new()
        {
            Namespace = symbol.GetContainingNamespace(),
            Modifiers = syntax.Modifiers.ToString(),
            Keyword = syntax.GetKeyword(),
            Identifier = syntax.Identifier.ValueText,
            TypeParameters = syntax.TypeParameterList?.ToString() ?? "",
            Arity = syntax.Arity
        };
}

/// <summary>
/// Cacheable data for a C# type. All data required to declare a valid partial type, including namespace and parent types.
/// </summary>
/// <param name="Type"></param>
/// <param name="Parents"></param>
internal record TypeFullData(TypeData Type, CacheStack<TypeBaseData> Parents)
{
    public static TypeFullData From(INamedTypeSymbol symbol, TypeDeclarationSyntax syntax) =>
        new(TypeData.From(symbol, syntax), GetParents(syntax));

    /// <summary>
    /// Recursively collects every parent of passed syntax node.
    /// </summary>
    /// <param name="syntax">The node to get every parent type of. The passed node itself is not included.</param>
    /// <returns>Array of every parent. Outermost first.</returns>
    public static CacheStack<TypeBaseData> GetParents(TypeDeclarationSyntax syntax)
    {
        static CacheStack<TypeBaseData> Core(TypeDeclarationSyntax syntax, CacheStack<TypeBaseData> stack)
        {
            return syntax.Parent switch
            {
                TypeDeclarationSyntax parentSyntax => Core(parentSyntax, stack.Push(TypeBaseData.From(parentSyntax))),
                _ => stack
            };
        }

        return Core(syntax, CacheStack<TypeBaseData>.Empty);
    }

    public Unit Accept(TypeVisitor typeEmitter)
    {
        typeEmitter.EmitTypeFullData(this);
        return unit;
    }
}
