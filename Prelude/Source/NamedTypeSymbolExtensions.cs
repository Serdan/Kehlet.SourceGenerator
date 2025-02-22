using Microsoft.CodeAnalysis;

namespace Kehlet.SourceGenerator;

public static class NamedTypeSymbolExtensions
{
    public static string GetContainingNamespace(this INamedTypeSymbol? self) =>
        self?.ContainingNamespace.ToDisplayString(
            SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted)) ?? "";
}
