#nullable enable
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace Kehlet.SourceGenerator;

internal static class NamedTypeSymbolExtensions
{
    [return: NotNullIfNotNull(nameof(symbol))]
    public static string? GetContainingNamespace(this INamedTypeSymbol? symbol) =>
        symbol?.ContainingNamespace.ToDisplayString(
            SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted));

    [return: NotNullIfNotNull(nameof(symbol))]
    public static string? FullMetadataName(this INamedTypeSymbol? symbol) =>
        symbol?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted));
}
