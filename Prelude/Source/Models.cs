using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Kehlet.SourceGenerator;

/// <summary>
/// Cacheable base data for a C# type.
/// </summary>
/// <param name="Keyword"></param>
/// <param name="Identifier"></param>
internal record TypeBaseData(string Modifiers, string Keyword, string Identifier, string TypeParameters, int Arity)
{
    public override string ToString() => $"{Modifiers} {Keyword} {Identifier}{TypeParameters}";

    public static TypeBaseData From(TypeDeclarationSyntax syntax) =>
        new(syntax.Modifiers.ToString(), syntax.GetKeyword(), syntax.Identifier.ValueText, syntax.TypeParameterList?.ToString() ?? "", syntax.Arity);
}

/// <summary>
/// Cacheable data for a C# type. This is sufficient to create a valid partial declaration.
/// </summary>
/// <param name="Namespace"></param>
/// <param name="BaseData"></param>
internal record TypeData(string Namespace, TypeBaseData BaseData)
{
    public string NamespaceUsing => string.IsNullOrWhiteSpace(Namespace) ? "" : $"using {Namespace};";

    public string NamespaceDeclaration => string.IsNullOrWhiteSpace(Namespace) ? "" : $"namespace {Namespace};";

    public string TypeDeclaration => BaseData.ToString();

    public string GetFileName(bool fullyQualified = false)
    {
        var name = "";
        if (fullyQualified && string.IsNullOrWhiteSpace(Namespace) is false)
        {
            name += Namespace + ".";
        }

        name += BaseData.Identifier;
        if (BaseData.Arity > 0)
        {
            name += $"`{BaseData.Arity}";
        }

        return name + ".g.cs";
    }

    public static TypeData From(INamedTypeSymbol symbol, TypeDeclarationSyntax syntax) =>
        new(symbol.GetContainingNamespace(), TypeBaseData.From(syntax));
}

/// <summary>
/// Cacheable wrapper for <see cref="Location"/>
/// </summary>
/// <param name="location"></param>
internal class SafeLocation(Location location)
{
    public Location Location { get; } = location.ToCacheable();

    public static SafeLocation From(Location location) => new(location);

    [return: NotNullIfNotNull(nameof(location))]
    public static implicit operator Location?(SafeLocation? location) => location?.Location;
}

internal interface IDiagnostic
{
    Unit Report(SourceProductionContext context);
}
