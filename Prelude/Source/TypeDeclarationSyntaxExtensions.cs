using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Kehlet.SourceGenerator;

internal static class TypeDeclarationSyntaxExtensions
{
    /// <summary>
    /// Get the declaration keyword as a string (class, struct, interface, record, record class, record struct)
    /// </summary>
    /// <param name="syntax"></param>
    /// <returns></returns>
    public static string GetKeyword(this TypeDeclarationSyntax syntax) =>
        syntax is RecordDeclarationSyntax record
            ? $"{record.Keyword.ValueText} {record.ClassOrStructKeyword}".Trim()
            : syntax.Keyword.ValueText;

    /// <summary>
    /// Identifier with any type parameters
    /// </summary>
    /// <param name="syntax"></param>
    /// <returns></returns>
    public static string GetFullIdentifier(this TypeDeclarationSyntax syntax) =>
        $"{syntax.Identifier}{syntax.TypeParameterList}";
}
