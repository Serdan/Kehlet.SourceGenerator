using System.Diagnostics.CodeAnalysis;
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
    public override string ToString() => $"{Modifiers} {Keyword} {Identifier}{TypeParameters}";

    public static TypeBaseData From(TypeDeclarationSyntax syntax) =>
        new(syntax.Modifiers.ToString(), syntax.GetKeyword(), syntax.Identifier.ValueText, syntax.TypeParameterList?.ToString() ?? "", syntax.Arity);
}

/// <summary>
/// Cacheable data for a C# type. This is sufficient to create a valid partial declaration.
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
    public string NamespaceUsing => string.IsNullOrWhiteSpace(Namespace) ? "" : $"using {Namespace};";

    public string NamespaceDeclaration => string.IsNullOrWhiteSpace(Namespace) ? "" : $"namespace {Namespace};";

    public string TypeDeclaration => base.ToString();

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
            symbol.GetContainingNamespace() ?? "");
}

/// <summary>
/// Cacheable wrapper for <see cref="Location"/>
/// </summary>
/// <param name="location"></param>
internal class SafeLocation(Location location) : IEquatable<SafeLocation>
{
    public Location Location { get; } = location.ToCacheable();

    [return: NotNullIfNotNull(nameof(location))]
    public static implicit operator Location?(SafeLocation? location) => location?.Location;

    public bool Equals(SafeLocation? other) =>
        unit switch
        {
            _ when other is null => false,
            _ when ReferenceEquals(this, other) => true,
            _ => Location.Equals(other.Location)
        };

    public override bool Equals(object? obj) => obj is SafeLocation location && Equals(location);

    public override int GetHashCode() => Location.GetHashCode();

    public static SafeLocation From(Location location) => new(location);
}

internal interface IDiagnostic
{
    Unit Report(SourceProductionContext context);
}
