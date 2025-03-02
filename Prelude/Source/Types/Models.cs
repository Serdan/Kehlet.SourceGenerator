using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Kehlet.SourceGenerator;

/// <summary>
/// Cacheable base data for a C# type.
/// </summary>
/// <param name="Modifiers"></param>
/// <param name="Keyword"></param>
/// <param name="Identifier"></param>
/// <param name="TypeParameters"></param>
/// <param name="Arity"></param>
internal record TypeBaseData(string Modifiers, string Keyword, string Identifier, string TypeParameters, int Arity)
{
    public string TypeDeclaration => $"{Modifiers} {Keyword} {Identifier}{TypeParameters}";

    public override string ToString() => TypeDeclaration;

    public static TypeBaseData From(TypeDeclarationSyntax syntax) =>
        new(syntax.Modifiers.ToString(), syntax.GetKeyword(), syntax.Identifier.ValueText, syntax.TypeParameterList?.ToString() ?? "", syntax.Arity);
}

/// <summary>
/// Cacheable data for a C# type. This is sufficient to create a valid partial declaration for unnested types.
/// </summary>
/// <param name="Modifiers"></param>
/// <param name="Keyword"></param>
/// <param name="Identifier"></param>
/// <param name="TypeParameters"></param>
/// <param name="Arity"></param>
/// <param name="Namespace"></param>
internal record TypeData(string Modifiers, string Keyword, string Identifier, string TypeParameters, int Arity, string Namespace)
    : TypeBaseData(Modifiers, Keyword, Identifier, TypeParameters, Arity)
{
    public string NamespaceDeclaration => string.IsNullOrWhiteSpace(Namespace) ? "" : $"namespace {Namespace};";

    public string GetFileName(bool fullyQualified = false)
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
        new(syntax.Modifiers.ToString(),
            syntax.GetKeyword(),
            syntax.Identifier.ValueText,
            syntax.TypeParameterList?.ToString() ?? "",
            syntax.Arity,
            symbol.GetContainingNamespace());
}

/// <summary>
/// All data required to declare a valid partial type, including namespace and parent types.
/// </summary>
/// <param name="Type"></param>
/// <param name="Parents"></param>
internal record TypeFullData(TypeData Type, ImmutableArray<TypeBaseData> Parents)
{
    public static TypeFullData From(INamedTypeSymbol symbol, TypeDeclarationSyntax syntax) =>
        new(TypeData.From(symbol, syntax), GetParents(syntax));

    /// <summary>
    /// Recursively collects every parent of passed syntax node.
    /// </summary>
    /// <param name="syntax">The node to get every parent type of. The passed node itself is not included.</param>
    /// <returns>Array of every parent. Outermost first.</returns>
    public static ImmutableArray<TypeBaseData> GetParents(TypeDeclarationSyntax syntax)
    {
        static ImmutableStack<TypeBaseData> Core(TypeDeclarationSyntax syntax, ImmutableStack<TypeBaseData> stack)
        {
            return syntax.Parent switch
            {
                TypeDeclarationSyntax parentSyntax => Core(parentSyntax, stack.Push(TypeBaseData.From(parentSyntax))),
                _ => [..stack]
            };
        }

        return [..Core(syntax, ImmutableStack<TypeBaseData>.Empty)];
    }

    public Unit Accept(TypeVisitor visitor)
    {
        visitor.VisitTypeFullData(this);
        return unit;
    }
}
