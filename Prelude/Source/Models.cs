using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Kehlet.SourceGenerator;

/// <summary>
/// Cacheable base data for a C# type.
/// </summary>
/// <param name="Keyword"></param>
/// <param name="Identifier"></param>
internal readonly record struct TypeBaseData(string Modifiers, string Keyword, string Identifier, string TypeParameters)
{
    public override string ToString() => $"{Modifiers} {Keyword} {Identifier}{TypeParameters}";

    public static TypeBaseData From(TypeDeclarationSyntax syntax) =>
        new(syntax.Modifiers.ToString(), syntax.GetKeyword(), syntax.Identifier.ValueText, syntax.TypeParameterList?.ToString() ?? "");
}

/// <summary>
/// Cacheable data for a C# type. This is sufficient to create a valid partial declaration.
/// </summary>
/// <param name="Namespace"></param>
/// <param name="BaseData"></param>
internal readonly record struct TypeData(string Namespace, TypeBaseData BaseData)
{
    public string NamespaceUsing => string.IsNullOrWhiteSpace(Namespace) ? "" : $"using {Namespace};";

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
}

internal sealed record DiagnosticData<TError>(TError Error, SafeLocation Location);
