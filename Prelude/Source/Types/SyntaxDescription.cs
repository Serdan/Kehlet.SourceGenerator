#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Kehlet.SourceGenerator;

internal enum TypeIdentifierKind
{
    Error,
    Identifier,
    Predefined
}

internal abstract record SyntaxDescription
{
    public abstract Option<TResult> Accept<TResult>(SyntaxDescriptionVisitor<TResult> visitor) where TResult : notnull;
    public abstract Unit Accept(SyntaxDescriptionWalker visitor);
}

[DebuggerDisplay("{Text}")]
internal record UsingDescription : SyntaxDescription
{
    public required string Text { get; init; }

    public override Option<TResult> Accept<TResult>(SyntaxDescriptionVisitor<TResult> visitor) =>
        visitor.VisitUsing(this);

    public override Unit Accept(SyntaxDescriptionWalker visitor) =>
        visitor.VisitUsing(this);
}

[DebuggerDisplay("{Identifier}")]
internal record TypeIdentifierDescription : SyntaxDescription
{
    public required string Identifier { get; init; }

    public required TypeIdentifierKind Kind { get; init; }

    public override Option<TResult> Accept<TResult>(SyntaxDescriptionVisitor<TResult> visitor) => visitor.VisitTypeIdentifier(this);
    public override Unit Accept(SyntaxDescriptionWalker visitor) => visitor.VisitTypeIdentifier(this);

    public static TypeIdentifierDescription Error => new() { Identifier = "?", Kind = TypeIdentifierKind.Error };
}

[DebuggerDisplay("{Modifiers}")]
[CollectionBuilder(typeof(ModifierListDescription), nameof(Create))]
internal record ModifierListDescription : SyntaxDescription, IEnumerable<string>
{
    public required CacheArray<string> Modifiers { get; init; }

    public static ModifierListDescription Empty { get; } = [];

    public static ModifierListDescription Create(ReadOnlySpan<string> items) =>
        new() { Modifiers = items.ToImmutableArray().ToCacheArray() };

    public override Option<TResult> Accept<TResult>(SyntaxDescriptionVisitor<TResult> visitor) => visitor.VisitModifierList(this);
    public override Unit Accept(SyntaxDescriptionWalker visitor) => visitor.VisitModifierList(this);

    public IEnumerator<string> GetEnumerator()
    {
        return ((IEnumerable<string>)Modifiers).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)Modifiers).GetEnumerator();
    }
}

[DebuggerDisplay("{Parameters}")]
internal record TypeParameterListDescription : SyntaxDescription
{
    public required Option<string> Parameters { get; init; }

    public static TypeParameterListDescription Empty => new() { Parameters = None };

    public override Option<TResult> Accept<TResult>(SyntaxDescriptionVisitor<TResult> visitor) => visitor.VisitTypeParameterList(this);
    public override Unit Accept(SyntaxDescriptionWalker visitor) => visitor.VisitTypeParameterList(this);
}

[DebuggerDisplay("{Identifier}")]
internal record ParameterDescription : SyntaxDescription
{
    public required ModifierListDescription Modifiers { get; init; }

    public required Option<TypeIdentifierDescription> Type { get; init; }

    public required string Identifier { get; init; }

    public override Option<TResult> Accept<TResult>(SyntaxDescriptionVisitor<TResult> visitor) => visitor.VisitParameter(this);
    public override Unit Accept(SyntaxDescriptionWalker visitor) => visitor.VisitParameter(this);
}

[DebuggerDisplay("{Parameters}")]
[CollectionBuilder(typeof(ParameterListDescription), nameof(Create))]
internal record ParameterListDescription : SyntaxDescription, IEnumerable<ParameterDescription>
{
    public required CacheArray<ParameterDescription> Parameters { get; init; }

    public static ParameterListDescription Empty { get; } = [];

    public static ParameterListDescription Create(ReadOnlySpan<ParameterDescription> items) =>
        new() { Parameters = items.ToImmutableArray().ToCacheArray() };

    public override Option<TResult> Accept<TResult>(SyntaxDescriptionVisitor<TResult> visitor) => visitor.VisitParameterList(this);
    public override Unit Accept(SyntaxDescriptionWalker visitor) => visitor.VisitParameterList(this);

    public IEnumerator<ParameterDescription> GetEnumerator()
    {
        return ((IEnumerable<ParameterDescription>)Parameters).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)Parameters).GetEnumerator();
    }
}

[DebuggerDisplay("{Parameters}")]
[CollectionBuilder(typeof(BracketedParameterListDescription), nameof(Create))]
internal record BracketedParameterListDescription : SyntaxDescription, IEnumerable<ParameterDescription>
{
    public required CacheArray<ParameterDescription> Parameters { get; init; }

    public static BracketedParameterListDescription Empty { get; } = [];

    public static BracketedParameterListDescription Create(ReadOnlySpan<ParameterDescription> items) =>
        new() { Parameters = items.ToImmutableArray().ToCacheArray() };

    public override Option<TResult> Accept<TResult>(SyntaxDescriptionVisitor<TResult> visitor) => visitor.VisitBracketedParameterList(this);
    public override Unit Accept(SyntaxDescriptionWalker visitor) => visitor.VisitBracketedParameterList(this);

    public IEnumerator<ParameterDescription> GetEnumerator()
    {
        return ((IEnumerable<ParameterDescription>)Parameters).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)Parameters).GetEnumerator();
    }
}

[DebuggerDisplay("{Modifiers}")]
internal abstract record MemberDescription : SyntaxDescription
{
    public bool IsTargetNode { get; init; }

    public virtual ModifierListDescription Modifiers { get; init; } = ModifierListDescription.Empty;
}

[DebuggerDisplay("{Identifier}")]
internal record MethodDescription : MemberDescription
{
    public required Option<TypeIdentifierDescription> ReturnType { get; init; }

    public required string Identifier { get; init; }

    public required TypeParameterListDescription TypeParameterList { get; init; }

    public required int Arity { get; init; }

    public required ParameterListDescription Parameters { get; init; }

    public string GetMethodDeclaration() =>
        $"{Modifiers} {ReturnType} {Identifier}{TypeParameterList}({Parameters})";

    public override Option<TResult> Accept<TResult>(SyntaxDescriptionVisitor<TResult> visitor) => visitor.VisitMethod(this);
    public override Unit Accept(SyntaxDescriptionWalker visitor) => visitor.VisitMethod(this);
}

[DebuggerDisplay("{Keyword}")]
internal record AccessorDescription : SyntaxDescription
{
    public required string Keyword { get; init; }

    public override Option<TResult> Accept<TResult>(SyntaxDescriptionVisitor<TResult> visitor) => visitor.VisitAccessor(this);
    public override Unit Accept(SyntaxDescriptionWalker visitor) => visitor.VisitAccessor(this);
}

[DebuggerDisplay("{Accessors}")]
[CollectionBuilder(typeof(AccessorListDescription), nameof(Create))]
internal record AccessorListDescription : SyntaxDescription, IEnumerable<AccessorDescription>
{
    public required CacheArray<AccessorDescription> Accessors { get; init; }

    public static AccessorListDescription Empty { get; } = [];

    public static AccessorListDescription Getter => [new() { Keyword = "get" }];

    public static AccessorListDescription Create(ReadOnlySpan<AccessorDescription> items) =>
        new() { Accessors = items.ToImmutableArray().ToCacheArray() };

    public override Option<TResult> Accept<TResult>(SyntaxDescriptionVisitor<TResult> visitor) => visitor.VisitAccessorList(this);
    public override Unit Accept(SyntaxDescriptionWalker visitor) => visitor.VisitAccessorList(this);

    public IEnumerator<AccessorDescription> GetEnumerator()
    {
        return ((IEnumerable<AccessorDescription>)Accessors).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)Accessors).GetEnumerator();
    }
}

internal abstract record BasePropertyDescription : MemberDescription
{
    public required Option<TypeIdentifierDescription> Type { get; init; }

    public required AccessorListDescription Accessors { get; init; }
}

[DebuggerDisplay("{Identifier}")]
internal record PropertyDescription : BasePropertyDescription
{
    public required string Identifier { get; init; }

    public override Option<TResult> Accept<TResult>(SyntaxDescriptionVisitor<TResult> visitor) => visitor.VisitProperty(this);
    public override Unit Accept(SyntaxDescriptionWalker visitor) => visitor.VisitProperty(this);
}

[DebuggerDisplay("{Parameters}")]
internal record IndexerDescription : BasePropertyDescription
{
    public required BracketedParameterListDescription Parameters { get; init; }

    public override Option<TResult> Accept<TResult>(SyntaxDescriptionVisitor<TResult> visitor) => visitor.VisitIndexer(this);
    public override Unit Accept(SyntaxDescriptionWalker visitor) => visitor.VisitIndexer(this);
}

[DebuggerDisplay("{Name}")]
internal record NamespaceDescription : MemberDescription
{
    public required string Name { get; init; }

    public required bool IsFileScoped { get; init; }

    public required CacheArray<UsingDescription> Usings { get; init; }

    public required CacheArray<MemberDescription> Members { get; init; }

    public NamespaceDescription WithMembers(params ReadOnlySpan<MemberDescription> members) =>
        this with { Members = members.ToImmutableArray().ToCacheArray() };

    public sealed override ModifierListDescription Modifiers { get; init; } = [];

    public override Option<TResult> Accept<TResult>(SyntaxDescriptionVisitor<TResult> visitor) => visitor.VisitNamespace(this);
    public override Unit Accept(SyntaxDescriptionWalker visitor) => visitor.VisitNamespace(this);
}

[DebuggerDisplay("{Identifier}")]
internal record NamedTypeDescription : MemberDescription
{
    public required string Identifier { get; init; }

    public required string Keyword { get; init; }

    public required TypeParameterListDescription TypeParameters { get; init; }

    public required int Arity { get; init; }

    public required CacheArray<MemberDescription> Members { get; init; }

    public NamedTypeDescription WithMembers(params ReadOnlySpan<MemberDescription> members) =>
        this with { Members = members.ToImmutableArray().ToCacheArray() };

    public override Option<TResult> Accept<TResult>(SyntaxDescriptionVisitor<TResult> visitor) => visitor.VisitNamedType(this);
    public override Unit Accept(SyntaxDescriptionWalker visitor) => visitor.VisitNamedType(this);
}

/// <summary>
/// Represents a single C# code file.
/// </summary>
internal record ModuleDescription : MemberDescription
{
    public required CacheArray<UsingDescription> Usings { get; init; }

    public required CacheArray<MemberDescription> Members { get; init; }

    public ModuleDescription WithMembers(params ReadOnlySpan<MemberDescription> members) =>
        this with { Members = members.ToImmutableArray().ToCacheArray() };

    public override Option<TResult> Accept<TResult>(SyntaxDescriptionVisitor<TResult> visitor) => visitor.VisitModule(this);
    public override Unit Accept(SyntaxDescriptionWalker visitor) => visitor.VisitModule(this);
}

internal static class SyntaxHelper
{
    public static ModifierListDescription ParseModifierList(SyntaxTokenList tokens) =>
        new() { Modifiers = tokens.Select(x => x.ValueText).ToCacheArray() };

    public static SyntaxDescription WithMembers(this SyntaxDescription node, params ReadOnlySpan<MemberDescription> members)
    {
        static CacheArray<MemberDescription> ToArray(ReadOnlySpan<MemberDescription> members) => members.ToImmutableArray().ToCacheArray();

        return node switch
        {
            ModuleDescription description => description with { Members = ToArray(members) },
            NamespaceDescription description => description with { Members = ToArray(members) },
            NamedTypeDescription description => description with { Members = ToArray(members) },
            _ => throw new InvalidOperationException()
        };
    }

    public static Option<ModuleDescription> GetTargetWithContext(MemberDeclarationSyntax target)
    {
        var walker = new SyntaxToDescriptionWalker(SyntaxToDescriptionWalkerConfiguration.Context);
        walker.Visit(target);
        return walker.Context;
    }

    public static Option<ModuleDescription> GetTargetWithAll(MemberDeclarationSyntax target)
    {
        var walker = new SyntaxToDescriptionWalker(SyntaxToDescriptionWalkerConfiguration.All);
        walker.Visit(target);
        return walker.Context;
    }

    public static Option<ModuleDescription> GetTarget(MemberDeclarationSyntax target, SyntaxToDescriptionWalkerConfiguration configuration)
    {
        var walker = new SyntaxToDescriptionWalker(configuration);
        walker.Visit(target);
        return walker.Context;
    }
}

internal abstract class BaseSyntaxVisitor : CSharpSyntaxVisitor<SyntaxDescription>
{
    protected virtual CacheArray<TResult> VisitSyntaxList<TSource, TResult>(SyntaxList<TSource> list) where TSource : SyntaxNode =>
        list.Select(Visit).OfType<TResult>().ToCacheArray();

    public override SyntaxDescription VisitUsingDirective(UsingDirectiveSyntax node) =>
        new UsingDescription { Text = node.ToFullString().Trim() };

    public virtual SyntaxDescription VisitBaseNamespaceDeclarationSyntax(BaseNamespaceDeclarationSyntax node)
    {
        return new NamespaceDescription
        {
            Name = node.Name.ToFullString().Trim(),
            IsFileScoped = node is FileScopedNamespaceDeclarationSyntax,
            Usings = VisitSyntaxList<UsingDirectiveSyntax, UsingDescription>(node.Usings),
            IsTargetNode = false,
            Members = []
        };
    }

    public override SyntaxDescription VisitNamespaceDeclaration(NamespaceDeclarationSyntax node) =>
        VisitBaseNamespaceDeclarationSyntax(node);

    public override SyntaxDescription VisitFileScopedNamespaceDeclaration(FileScopedNamespaceDeclarationSyntax node) =>
        VisitBaseNamespaceDeclarationSyntax(node);

    public override SyntaxDescription VisitCompilationUnit(CompilationUnitSyntax node)
    {
        return new ModuleDescription { Usings = VisitSyntaxList<UsingDirectiveSyntax, UsingDescription>(node.Usings), IsTargetNode = false, Members = [] };
    }

    public virtual SyntaxDescription? VisitTypeDeclaration(TypeDeclarationSyntax node)
    {
        return new NamedTypeDescription
        {
            Modifiers = SyntaxHelper.ParseModifierList(node.Modifiers),
            Keyword = node.GetKeyword(),
            Identifier = node.Identifier.ValueText,
            TypeParameters = Visit(node.TypeParameterList) as TypeParameterListDescription ?? TypeParameterListDescription.Empty,
            Arity = node.TypeParameterList?.Parameters.Count ?? 0,
            IsTargetNode = false,
            Members = []
        };
    }

    public override SyntaxDescription? VisitClassDeclaration(ClassDeclarationSyntax node) => VisitTypeDeclaration(node);
    public override SyntaxDescription? VisitStructDeclaration(StructDeclarationSyntax node) => VisitTypeDeclaration(node);
    public override SyntaxDescription? VisitRecordDeclaration(RecordDeclarationSyntax node) => VisitTypeDeclaration(node);

    public override SyntaxDescription VisitTypeParameterList(TypeParameterListSyntax node) =>
        new TypeParameterListDescription { Parameters = node.ToFullString().Trim() };

    public override SyntaxDescription VisitParameterList(ParameterListSyntax node) =>
        new ParameterListDescription { Parameters = node.Parameters.Select(Visit).OfType<ParameterDescription>().ToCacheArray() };

    public override SyntaxDescription VisitBracketedParameterList(BracketedParameterListSyntax node) =>
        new BracketedParameterListDescription { Parameters = node.Parameters.Select(Visit).OfType<ParameterDescription>().ToCacheArray() };

    public override SyntaxDescription VisitParameter(ParameterSyntax node) =>
        new ParameterDescription { Identifier = node.Identifier.ValueText, Type = (Visit(node.Type) as TypeIdentifierDescription).ToOption(), Modifiers = SyntaxHelper.ParseModifierList(node.Modifiers) };

    public override SyntaxDescription VisitIdentifierName(IdentifierNameSyntax node) =>
        new TypeIdentifierDescription { Identifier = node.Identifier.ValueText, Kind = TypeIdentifierKind.Identifier };

    public override SyntaxDescription VisitPredefinedType(PredefinedTypeSyntax node) =>
        new TypeIdentifierDescription { Identifier = node.Keyword.ValueText, Kind = TypeIdentifierKind.Predefined };

    public override SyntaxDescription VisitAccessorList(AccessorListSyntax node) =>
        new AccessorListDescription { Accessors = node.Accessors.Select(Visit).OfType<AccessorDescription>().ToCacheArray() };

    public override SyntaxDescription VisitAccessorDeclaration(AccessorDeclarationSyntax node) =>
        new AccessorDescription { Keyword = node.Keyword.ValueText };
}

internal sealed class ContextWalker : BaseSyntaxVisitor
{
    private Option<ModuleDescription> module = None;
    private readonly Queue<MemberDescription> members = new();

    public override SyntaxDescription VisitTypeDeclaration(TypeDeclarationSyntax node)
    {
        var self = (NamedTypeDescription)base.VisitTypeDeclaration(node)!;
        members.Enqueue(self);
        Visit(node.Parent);
        return self;
    }

    public override SyntaxDescription VisitBaseNamespaceDeclarationSyntax(BaseNamespaceDeclarationSyntax node)
    {
        var self = (NamespaceDescription)base.VisitBaseNamespaceDeclarationSyntax(node);
        members.Enqueue(self);
        Visit(node.Parent);
        return self;
    }

    public override SyntaxDescription VisitCompilationUnit(CompilationUnitSyntax node)
    {
        var root = (ModuleDescription)base.VisitCompilationUnit(node);
        module = root;
        return root;
    }

    public Option<ModuleDescription> GetRoot(MemberDescription child)
    {
        if (module.IsNone)
        {
            return None;
        }

        var current = (MemberDescription)members.Dequeue().WithMembers(child);

        while (members.Count > 0 && members.Dequeue() is { } member)
        {
            current = (MemberDescription)member.WithMembers(current);
        }

        return module.UnsafeValue.WithMembers(current);
    }
}

internal sealed record SyntaxToDescriptionWalkerConfiguration
{
    /// <summary>
    /// If true will include type members, such as methods and properties.
    /// </summary>
    public required bool IncludeTypeMembers { get; init; }

    /// <summary>
    /// If true will include nested types.
    /// </summary>
    public required bool IncludeNestedTypes { get; init; }

    /// <summary>
    /// If true will include parent types, namespaces and usings.
    /// </summary>
    public required bool IncludeContext { get; init; }

    /// <summary>
    /// Includes all nested types, type members and context.
    /// </summary>
    public static readonly SyntaxToDescriptionWalkerConfiguration All = new() { IncludeNestedTypes = true, IncludeTypeMembers = true, IncludeContext = true };

    /// <summary>
    /// Doesn't include any nested types or type members, but still includes context.
    /// </summary>
    public static readonly SyntaxToDescriptionWalkerConfiguration Context = new() { IncludeNestedTypes = false, IncludeTypeMembers = false, IncludeContext = true };

    /// <summary>
    /// Only includes the type passed to the Visit method, but not its members.
    /// </summary>
    public static readonly SyntaxToDescriptionWalkerConfiguration Single = new() { IncludeContext = false, IncludeNestedTypes = false, IncludeTypeMembers = false };
}

internal class SyntaxToDescriptionWalker(SyntaxToDescriptionWalkerConfiguration configuration) : BaseSyntaxVisitor
{
    private bool includeContext = configuration.IncludeContext;
    private bool isTarget = true;

    public Option<ModuleDescription> Context { get; private set; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SyntaxDescription UpdateContext(MemberDeclarationSyntax node, MemberDescription description)
    {
        if (isTarget)
        {
            description = description with { IsTargetNode = true };
            isTarget = false;
        }

        if (!includeContext) return description;

        includeContext = false;
        var walker = new ContextWalker();
        walker.Visit(node.Parent);
        Context = walker.GetRoot(description);
        return description;
    }

    private static bool IsTypeMember(MemberDeclarationSyntax node) =>
        node is BaseMethodDeclarationSyntax or BasePropertyDeclarationSyntax;

    public override SyntaxDescription? VisitTypeDeclaration(TypeDeclarationSyntax node)
    {
        if (node.Modifiers.Any(SyntaxKind.PartialKeyword) is false)
        {
            return null;
        }

        CacheArray<MemberDescription> VisitMembers()
        {
            if (configuration is { IncludeNestedTypes: false, IncludeTypeMembers: false })
            {
                return [];
            }

            var members = ImmutableArray.CreateBuilder<MemberDescription>(node.Members.Count);
            foreach (var member in node.Members)
            {
                if (configuration.IncludeTypeMembers is false && IsTypeMember(member))
                {
                    continue;
                }

                if (configuration.IncludeNestedTypes is false && member is TypeDeclarationSyntax)
                {
                    continue;
                }

                if (Visit(member) is not MemberDescription m)
                {
                    continue;
                }

                members.Add(m);
            }

            return members.ToCacheArray();
        }

        var description = (NamedTypeDescription)base.VisitTypeDeclaration(node)! with { Members = VisitMembers() };

        return UpdateContext(node, description);
    }

    public override SyntaxDescription? VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        if (node.Modifiers.Any(SyntaxKind.PartialKeyword) is false)
        {
            return null;
        }

        var description = new MethodDescription
        {
            Modifiers = SyntaxHelper.ParseModifierList(node.Modifiers),
            ReturnType = (Visit(node.ReturnType) as TypeIdentifierDescription).ToOption(),
            Identifier = node.Identifier.ValueText,
            TypeParameterList = Visit(node.TypeParameterList) as TypeParameterListDescription ?? TypeParameterListDescription.Empty,
            Parameters = Visit(node.ParameterList) as ParameterListDescription ?? [],
            Arity = node.Arity
        };

        return UpdateContext(node, description);
    }

    public override SyntaxDescription? VisitPropertyDeclaration(PropertyDeclarationSyntax node)
    {
        if (node.Modifiers.Any(SyntaxKind.PartialKeyword) is false)
        {
            return null;
        }

        var description = new PropertyDescription
        {
            Modifiers = SyntaxHelper.ParseModifierList(node.Modifiers),
            Type = (Visit(node.Type) as TypeIdentifierDescription).ToOption(),
            Identifier = node.Identifier.ValueText,
            Accessors = Visit(node.AccessorList) as AccessorListDescription ?? AccessorListDescription.Getter
        };

        return UpdateContext(node, description);
    }

    public override SyntaxDescription? VisitIndexerDeclaration(IndexerDeclarationSyntax node)
    {
        if (node.Modifiers.Any(SyntaxKind.PartialKeyword) is false)
        {
            return null;
        }

        var description = new IndexerDescription
        {
            Modifiers = SyntaxHelper.ParseModifierList(node.Modifiers),
            Type = (Visit(node.Type) as TypeIdentifierDescription).ToOption(),
            Parameters = Visit(node.ParameterList) as BracketedParameterListDescription ?? [],
            Accessors = Visit(node.AccessorList) as AccessorListDescription ?? AccessorListDescription.Getter
        };

        return UpdateContext(node, description);
    }
}

/// <summary>
/// Visits only the single <see cref="SyntaxDescription"/> passed into its Visit method.
/// </summary>
internal abstract class SyntaxDescriptionVisitor<TResult> where TResult : notnull
{
    public virtual Option<TResult> Visit(SyntaxDescription description) => description.Accept(this);
    public virtual Option<TResult> DefaultVisit(SyntaxDescription description) => None;
    public virtual Option<TResult> VisitTypeIdentifier(TypeIdentifierDescription description) => DefaultVisit(description);
    public virtual Option<TResult> VisitModifierList(ModifierListDescription description) => DefaultVisit(description);
    public virtual Option<TResult> VisitTypeParameterList(TypeParameterListDescription description) => DefaultVisit(description);
    public virtual Option<TResult> VisitParameter(ParameterDescription description) => DefaultVisit(description);
    public virtual Option<TResult> VisitParameterList(ParameterListDescription description) => DefaultVisit(description);
    public virtual Option<TResult> VisitBracketedParameterList(BracketedParameterListDescription description) => DefaultVisit(description);
    public virtual Option<TResult> VisitMethod(MethodDescription description) => DefaultVisit(description);
    public virtual Option<TResult> VisitAccessor(AccessorDescription description) => DefaultVisit(description);
    public virtual Option<TResult> VisitAccessorList(AccessorListDescription description) => DefaultVisit(description);
    public virtual Option<TResult> VisitProperty(PropertyDescription description) => DefaultVisit(description);
    public virtual Option<TResult> VisitIndexer(IndexerDescription description) => DefaultVisit(description);
    public virtual Option<TResult> VisitNamedType(NamedTypeDescription description) => DefaultVisit(description);
    public virtual Option<TResult> VisitModule(ModuleDescription description) => DefaultVisit(description);
    public virtual Option<TResult> VisitUsing(UsingDescription description) => DefaultVisit(description);
    public virtual Option<TResult> VisitNamespace(NamespaceDescription description) => DefaultVisit(description);
}

internal class SyntaxDescriptionWalker
{
    public virtual Unit Visit(SyntaxDescription description) => description.Accept(this);

    public virtual Unit DefaultVisit(SyntaxDescription description) => unit;

    public virtual Unit VisitTypeIdentifier(TypeIdentifierDescription description) => DefaultVisit(description);
    public virtual Unit VisitModifierList(ModifierListDescription description) => DefaultVisit(description);
    public virtual Unit VisitTypeParameterList(TypeParameterListDescription description) => DefaultVisit(description);

    public virtual Unit VisitParameter(ParameterDescription description)
    {
        Visit(description.Modifiers);
        description.Type.Map(Visit);

        return unit;
    }

    public virtual Unit VisitParameterList(ParameterListDescription description)
    {
        foreach (var parameter in description.Parameters)
        {
            Visit(parameter);
        }

        return unit;
    }

    public virtual Unit VisitBracketedParameterList(BracketedParameterListDescription description)
    {
        foreach (var parameter in description.Parameters)
        {
            Visit(parameter);
        }

        return unit;
    }

    public virtual Unit VisitMethod(MethodDescription description)
    {
        Visit(description.Modifiers);
        Visit(description.Parameters);
        Visit(description.TypeParameterList);
        Visit(description.ReturnType.DefaultValue(TypeIdentifierDescription.Error));

        return unit;
    }

    public virtual Unit VisitAccessor(AccessorDescription description) => DefaultVisit(description);

    public virtual Unit VisitAccessorList(AccessorListDescription description)
    {
        foreach (var accessor in description.Accessors)
        {
            Visit(accessor);
        }

        return unit;
    }

    public virtual Unit VisitProperty(PropertyDescription description)
    {
        Visit(description.Modifiers);
        Visit(description.Accessors);
        Visit(description.Type.DefaultValue(TypeIdentifierDescription.Error));

        return unit;
    }

    public virtual Unit VisitIndexer(IndexerDescription description)
    {
        Visit(description.Modifiers);
        Visit(description.Accessors);
        Visit(description.Type.DefaultValue(TypeIdentifierDescription.Error));

        return unit;
    }

    public virtual Unit VisitNamedType(NamedTypeDescription description)
    {
        Visit(description.Modifiers);
        Visit(description.TypeParameters);
        foreach (var member in description.Members)
        {
            Visit(member);
        }

        return unit;
    }

    public virtual Unit VisitModule(ModuleDescription description)
    {
        foreach (var member in description.Members)
        {
            Visit(member);
        }

        return unit;
    }

    public virtual Unit VisitUsing(UsingDescription description) => DefaultVisit(description);

    public virtual Unit VisitNamespace(NamespaceDescription description)
    {
        foreach (var member in description.Members)
        {
            Visit(member);
        }

        return unit;
    }
}

internal class SyntaxDescriptionEmitter(Emitter emitter) : SyntaxDescriptionWalker
{
    protected Emitter Emitter { get; } = emitter;

    public SyntaxDescriptionEmitter() : this(new()) { }

    public override Unit VisitTypeIdentifier(TypeIdentifierDescription description)
    {
        return Emitter * description.Identifier * " " >> unit;
    }

    public override Unit VisitModifierList(ModifierListDescription description)
    {
        foreach (var modifier in description.Modifiers)
        {
            _ = Emitter * modifier * " ";
        }

        return unit;
    }

    public override Unit VisitTypeParameterList(TypeParameterListDescription description)
    {
        if (description.Parameters.IsNone)
        {
            return unit;
        }

        return Emitter * description.Parameters.UnsafeValue >> unit;
    }

    public override Unit VisitParameter(ParameterDescription description)
    {
        base.VisitParameter(description);
        return Emitter * description.Identifier >> unit;
    }

    public override Unit VisitParameterList(ParameterListDescription description)
    {
        Emitter.Append("(");

        var count = description.Parameters.Count;
        for (var i = 0; i < description.Parameters.Count; i++)
        {
            var parameter = description.Parameters[i];
            Visit(parameter);
            if (i < count - 1)
            {
                Emitter.Append(", ");
            }
        }

        return Emitter * ")" >> unit;
    }

    public override Unit VisitBracketedParameterList(BracketedParameterListDescription description)
    {
        Emitter.Append("[");

        var count = description.Parameters.Count;
        for (var i = 0; i < description.Parameters.Count; i++)
        {
            var parameter = description.Parameters[i];
            Visit(parameter);
            if (i < count - 1)
            {
                Emitter.Append(", ");
            }
        }

        return Emitter * "]" >> unit;
    }

    public virtual Unit VisitMethodBody(MethodDescription description)
    {
        return Emitter * " { }" / unit >> unit;
    }

    public override Unit VisitMethod(MethodDescription description)
    {
        Visit(description.Modifiers);
        Visit(description.ReturnType.DefaultValue(TypeIdentifierDescription.Error));
        Emitter.Append(description.Identifier);
        Visit(description.TypeParameterList);
        Visit(description.Parameters);
        VisitMethodBody(description);

        return unit;
    }

    public override Unit VisitAccessor(AccessorDescription description) =>
        Emitter * description.Keyword * ";" >> unit;

    public override Unit VisitAccessorList(AccessorListDescription description)
    {
        Emitter.Append("{ ");
        foreach (var accessor in description.Accessors)
        {
            Visit(accessor);
            Emitter.Append(" ");
        }

        return Emitter.Append("}") >> unit;
    }

    public override Unit VisitProperty(PropertyDescription description)
    {
        Visit(description.Modifiers);
        Visit(description.Type.DefaultValue(TypeIdentifierDescription.Error));
        Emitter.Append(description.Identifier).Append(" ");
        Visit(description.Accessors);
        Emitter.NewLine();

        return unit;
    }

    public override Unit VisitIndexer(IndexerDescription description)
    {
        Visit(description.Modifiers);
        Visit(description.Type.DefaultValue(TypeIdentifierDescription.Error));
        Emitter.Append("this");
        Visit(description.Parameters);
        Emitter.Append(" ");
        Visit(description.Accessors);
        Emitter.NewLine();

        return unit;
    }

    public virtual Unit VisitNamedTypeBody(NamedTypeDescription description) => DefaultVisit(description);

    public override Unit VisitNamedType(NamedTypeDescription description)
    {
        Visit(description.Modifiers);
        _ = Emitter * description.Keyword * " " * description.Identifier;
        Visit(description.TypeParameters);
        _ = Emitter / "{" / Indent.Increment;

        VisitNamedTypeBody(description);
        foreach (var member in description.Members)
        {
            Visit(member);
        }

        Emitter.CloseBrace();

        return unit;
    }

    public override Unit VisitNamespace(NamespaceDescription description)
    {
        Emitter.Append("namespace ").Append(description.Name);
        _ = description.IsFileScoped switch
        {
            true => Emitter.LineLine(";"),
            false => Emitter.NewLine().OpenBrace()
        };

        foreach (var @using in description.Usings)
        {
            Visit(@using);
        }

        if (description.Usings.Count > 0)
        {
            Emitter.NewLine();
        }

        foreach (var member in description.Members)
        {
            Visit(member);
        }

        if (description.IsFileScoped is false)
        {
            Emitter.CloseBrace();
        }

        return unit;
    }

    public override Unit VisitUsing(UsingDescription description) => Emitter.Line(description.Text).Ignore();

    public virtual Unit VisitUsingsList(CacheArray<UsingDescription> usings)
    {
        foreach (var @using in usings)
        {
            Visit(@using);
        }

        return unit;
    }

    public override Unit VisitModule(ModuleDescription description)
    {
        Emitter.NullableDirective();
        VisitUsingsList(description.Usings);

        if (description.Usings.Count > 0)
        {
            Emitter.NewLine();
        }

        for (var i = 0; i < description.Members.Count; i++)
        {
            var member = description.Members[i];
            Visit(member);

            if (description.Members.Count + 1 < i)
            {
                Emitter.NewLine();
            }
        }

        return unit;
    }

    public override string ToString() => Emitter.ToString();
}