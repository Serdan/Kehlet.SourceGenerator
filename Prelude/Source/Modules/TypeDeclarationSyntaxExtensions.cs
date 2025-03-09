#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
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

    private static readonly ConcurrentDictionary<string, (string, string)> Names = new();

    private static (string simpleName, string simpleNameAttribute) GetSimpleName(string name)
    {
        Debug.Assert(name.Length > 0);

        if (Names.TryGetValue(name, out var result))
        {
            return result;
        }

        const char dot = '.';
        const string attribute = "Attribute";
        var attributeSpan = attribute.AsSpan();
        var nameSpan = name.AsSpan();
        var lastDotIndex = -1;
        for (var i = nameSpan.Length - 1; i >= 0; i--)
        {
            if (nameSpan[i] != dot)
            {
                continue;
            }

            lastDotIndex = i;
            break;
        }

        (string, string) tuple;
        if (nameSpan.EndsWith(attributeSpan, StringComparison.Ordinal))
        {
            var simpleNameAttribute = nameSpan[(lastDotIndex + 1)..];
            var simpleName = simpleNameAttribute[..^attributeSpan.Length];
            tuple = (simpleName.ToString(), simpleNameAttribute.ToString());
        }
        else
        {
            var simpleName = nameSpan[(lastDotIndex + 1)..].ToString();
            var simpleNameAttribute = simpleName + attribute;
            tuple = (simpleName, simpleNameAttribute);
        }

        Names.TryAdd(name, tuple);
        return tuple;
    }

    private static ImmutableArray<AttributeData> GetAttributesWithName(
        this TypeDeclarationSyntax syntax, SemanticModel semanticModel, string fullMetadataName, bool returnFirst)
    {
        if (string.IsNullOrWhiteSpace(fullMetadataName))
        {
            throw new ArgumentException("Empty name given", nameof(fullMetadataName));
        }

        ISymbol? typeSymbol = null;
        ImmutableArray<(AttributeData, SyntaxNode)>? attributeDatas = null;

        var builder = ImmutableArray.CreateBuilder<AttributeData>();
        var (simpleName, simpleNameAttr) = GetSimpleName(fullMetadataName);

        foreach (var attributeList in syntax.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                var attributeName = attribute.Name.ToString();
                if (attributeName != simpleName && attributeName != simpleNameAttr)
                {
                    continue;
                }

                typeSymbol ??= semanticModel.GetDeclaredSymbol(syntax);
                // We assume it's possible to get the symbol, but if it isn't we have to bail.
                if (typeSymbol is null)
                {
                    return [];
                }

                attributeDatas ??= typeSymbol.GetAttributes()
                                             .Where(x => x.ApplicationSyntaxReference is not null)
                                             .Select(attributeData => (attributeData, attributeData.ApplicationSyntaxReference!.GetSyntax()))
                                             .ToImmutableArray();

                foreach (var (attributeData, syntaxNode) in attributeDatas)
                {
                    if (syntaxNode != attribute)
                    {
                        continue;
                    }

                    builder.Add(attributeData);
                    if (returnFirst)
                    {
                        return builder.ToImmutable();
                    }

                    break;
                }
            }
        }

        return builder.ToImmutable();
    }

    public static ImmutableArray<AttributeData> GetAttributesWithName(
        this TypeDeclarationSyntax syntax, SemanticModel semanticModel, string fullMetadataName) =>
        GetAttributesWithName(syntax, semanticModel, fullMetadataName, false);

    public static Option<AttributeData> GetAttributeWithName(this TypeDeclarationSyntax syntax, SemanticModel semanticModel, string fullMetadataName)
    {
        var result = GetAttributesWithName(syntax, semanticModel, fullMetadataName, true);
        if (result.IsEmpty)
        {
            return None;
        }

        return result[0];
    }

    public static bool HasAttributeWithName(this TypeDeclarationSyntax syntax, SemanticModel semanticModel, string fullMetadataName) =>
        GetAttributeWithName(syntax, semanticModel, fullMetadataName).IsSome;
}
