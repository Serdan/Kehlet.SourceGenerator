using Microsoft.CodeAnalysis;

namespace Kehlet.SourceGenerator;

public static class NamedTypeSymbolExtensions
{
    public static string GetContainingNamespace(this INamedTypeSymbol? self) =>
        self?.ContainingNamespace is { IsGlobalNamespace: false } ns ? ns.ToDisplayString() : "";
}
