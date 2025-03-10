﻿#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
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
}

[DebuggerDisplay("{Text}")]
internal record UsingDescription : SyntaxDescription
{
    public required string Text { get; init; }

    public override Option<TResult> Accept<TResult>(SyntaxDescriptionVisitor<TResult> visitor) =>
        visitor.VisitUsing(this);
}

[DebuggerDisplay("{Identifier}")]
internal record TypeIdentifierDescription : SyntaxDescription
{
    public required string Identifier { get; init; }

    public required TypeIdentifierKind Kind { get; init; }

    public override Option<TResult> Accept<TResult>(SyntaxDescriptionVisitor<TResult> visitor) => visitor.VisitTypeIdentifier(this);

    public static TypeIdentifierDescription Error => new()
    {
        Identifier = "MISSING",
        Kind = TypeIdentifierKind.Error
    };
}

[DebuggerDisplay("{Modifiers}")]
internal record ModifierListDescription : SyntaxDescription
{
    public required CacheArray<string> Modifiers { get; init; }

    public static ModifierListDescription Empty => new() { Modifiers = [] };

    public override Option<TResult> Accept<TResult>(SyntaxDescriptionVisitor<TResult> visitor) => visitor.VisitModifierList(this);
}

[DebuggerDisplay("{Parameters}")]
internal record TypeParameterListDescription : SyntaxDescription
{
    public required Option<string> Parameters { get; init; }

    public static TypeParameterListDescription Empty => new() { Parameters = None };

    public override Option<TResult> Accept<TResult>(SyntaxDescriptionVisitor<TResult> visitor) => visitor.VisitTypeParameterList(this);
}

[DebuggerDisplay("{Identifier}")]
internal record ParameterDescription : SyntaxDescription
{
    public required ModifierListDescription Modifiers { get; init; }

    public required Option<TypeIdentifierDescription> Type { get; init; }

    public required string Identifier { get; init; }

    public override Option<TResult> Accept<TResult>(SyntaxDescriptionVisitor<TResult> visitor) => visitor.VisitParameter(this);
}

[DebuggerDisplay("{Parameters}")]
internal record ParameterListDescription : SyntaxDescription
{
    public required CacheArray<ParameterDescription> Parameters { get; init; }

    public static ParameterListDescription Empty => new() { Parameters = [] };

    public override Option<TResult> Accept<TResult>(SyntaxDescriptionVisitor<TResult> visitor) => visitor.VisitParameterList(this);
}

[DebuggerDisplay("{Parameters}")]
internal record BracketedParameterListDescription : SyntaxDescription
{
    public required CacheArray<ParameterDescription> Parameters { get; init; }

    public static BracketedParameterListDescription Empty => new() { Parameters = [] };

    public override Option<TResult> Accept<TResult>(SyntaxDescriptionVisitor<TResult> visitor) => visitor.VisitBracketedParameterList(this);
}

[DebuggerDisplay("{Modifiers}")]
internal abstract record MemberDescription : SyntaxDescription
{
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
}

[DebuggerDisplay("{Keyword}")]
internal record AccessorDescription : SyntaxDescription
{
    public required string Keyword { get; init; }

    public override Option<TResult> Accept<TResult>(SyntaxDescriptionVisitor<TResult> visitor) => visitor.VisitAccessor(this);
}

[DebuggerDisplay("{Accessors}")]
internal record AccessorListDescription : SyntaxDescription
{
    public required CacheArray<AccessorDescription> Accessors { get; init; }

    public static AccessorListDescription Empty => new() { Accessors = [] };

    public static AccessorListDescription Getter => new() { Accessors = [new() { Keyword = "get" }] };

    public override Option<TResult> Accept<TResult>(SyntaxDescriptionVisitor<TResult> visitor) => visitor.VisitAccessorList(this);
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
}

[DebuggerDisplay("{Parameters}")]
internal record IndexerDescription : BasePropertyDescription
{
    public required BracketedParameterListDescription Parameters { get; init; }

    public override Option<TResult> Accept<TResult>(SyntaxDescriptionVisitor<TResult> visitor) => visitor.VisitIndexer(this);
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

    public sealed override ModifierListDescription Modifiers { get; init; } = ModifierListDescription.Empty;

    public override Option<TResult> Accept<TResult>(SyntaxDescriptionVisitor<TResult> visitor) => visitor.VisitNamespace(this);
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
}

internal record ModuleDescription : MemberDescription
{
    public required CacheArray<UsingDescription> Usings { get; init; }

    public required CacheArray<MemberDescription> Members { get; init; }

    public ModuleDescription WithMembers(params ReadOnlySpan<MemberDescription> members) =>
        this with { Members = members.ToImmutableArray().ToCacheArray() };

    public override Option<TResult> Accept<TResult>(SyntaxDescriptionVisitor<TResult> visitor) => visitor.VisitModule(this);
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
            ModuleDescription description    => description with { Members = ToArray(members) },
            NamespaceDescription description => description with { Members = ToArray(members) },
            NamedTypeDescription description => description with { Members = ToArray(members) },
            _                                => throw new InvalidOperationException()
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
            Members = []
        };
    }

    public override SyntaxDescription VisitNamespaceDeclaration(NamespaceDeclarationSyntax node) =>
        VisitBaseNamespaceDeclarationSyntax(node);

    public override SyntaxDescription VisitFileScopedNamespaceDeclaration(FileScopedNamespaceDeclarationSyntax node) =>
        VisitBaseNamespaceDeclarationSyntax(node);

    public override SyntaxDescription VisitCompilationUnit(CompilationUnitSyntax node)
    {
        return new ModuleDescription
        {
            Usings = VisitSyntaxList<UsingDirectiveSyntax, UsingDescription>(node.Usings),
            Members = []
        };
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
            Members = []
        };
    }

    public override SyntaxDescription? VisitClassDeclaration(ClassDeclarationSyntax node) => VisitTypeDeclaration(node);
    public override SyntaxDescription? VisitStructDeclaration(StructDeclarationSyntax node) => VisitTypeDeclaration(node);
    public override SyntaxDescription? VisitRecordDeclaration(RecordDeclarationSyntax node) => VisitTypeDeclaration(node);

    public override SyntaxDescription VisitTypeParameterList(TypeParameterListSyntax node) =>
        new TypeParameterListDescription
            { Parameters = node.ToFullString().Trim() };

    public override SyntaxDescription VisitParameterList(ParameterListSyntax node) =>
        new ParameterListDescription
            { Parameters = node.Parameters.Select(Visit).OfType<ParameterDescription>().ToCacheArray() };

    public override SyntaxDescription VisitBracketedParameterList(BracketedParameterListSyntax node) =>
        new BracketedParameterListDescription
            { Parameters = node.Parameters.Select(Visit).OfType<ParameterDescription>().ToCacheArray() };

    public override SyntaxDescription VisitParameter(ParameterSyntax node) =>
        new ParameterDescription
        {
            Identifier = node.Identifier.ValueText,
            Type = (Visit(node.Type) as TypeIdentifierDescription).ToOption(),
            Modifiers = SyntaxHelper.ParseModifierList(node.Modifiers)
        };

    public override SyntaxDescription VisitIdentifierName(IdentifierNameSyntax node) =>
        new TypeIdentifierDescription
        {
            Identifier = node.Identifier.ValueText,
            Kind = TypeIdentifierKind.Identifier
        };

    public override SyntaxDescription VisitPredefinedType(PredefinedTypeSyntax node) =>
        new TypeIdentifierDescription
        {
            Identifier = node.Keyword.ValueText,
            Kind = TypeIdentifierKind.Predefined
        };

    public override SyntaxDescription VisitAccessorList(AccessorListSyntax node) =>
        new AccessorListDescription
            { Accessors = node.Accessors.Select(Visit).OfType<AccessorDescription>().ToCacheArray() };

    public override SyntaxDescription VisitAccessorDeclaration(AccessorDeclarationSyntax node) =>
        new AccessorDescription { Keyword = node.Keyword.ValueText };
}

internal sealed class ContextWalker : BaseSyntaxVisitor
{
    private Option<ModuleDescription> module = None;
    private readonly Queue<MemberDescription> members = new();

    public override SyntaxDescription VisitTypeDeclaration(TypeDeclarationSyntax node)
    {
        var self = (NamedTypeDescription) base.VisitTypeDeclaration(node)!;
        members.Enqueue(self);
        Visit(node.Parent);
        return self;
    }

    public override SyntaxDescription VisitBaseNamespaceDeclarationSyntax(BaseNamespaceDeclarationSyntax node)
    {
        var self = (NamespaceDescription) base.VisitBaseNamespaceDeclarationSyntax(node);
        members.Enqueue(self);
        Visit(node.Parent);
        return self;
    }

    public override SyntaxDescription VisitCompilationUnit(CompilationUnitSyntax node)
    {
        var root = (ModuleDescription) base.VisitCompilationUnit(node);
        module = root;
        return root;
    }

    public Option<ModuleDescription> GetRoot(NamedTypeDescription child)
    {
        if (module.IsNone)
        {
            return None;
        }

        var current = (MemberDescription) members.Dequeue().WithMembers(child);

        while (members.Count > 0 && members.Dequeue() is { } member)
        {
            current = (MemberDescription) member.WithMembers(current);
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
    public static readonly SyntaxToDescriptionWalkerConfiguration All = new()
    {
        IncludeNestedTypes = true,
        IncludeTypeMembers = true,
        IncludeContext = true
    };

    /// <summary>
    /// Doesn't include any nested types or type members, but still includes context.
    /// </summary>
    public static readonly SyntaxToDescriptionWalkerConfiguration Context = new()
    {
        IncludeNestedTypes = false,
        IncludeTypeMembers = false,
        IncludeContext = true
    };

    /// <summary>
    /// Only includes the type passed to the Visit method, but not its members.
    /// </summary>
    public static readonly SyntaxToDescriptionWalkerConfiguration Single = new()
    {
        IncludeContext = false,
        IncludeNestedTypes = false,
        IncludeTypeMembers = false
    };
}

internal class SyntaxToDescriptionWalker(SyntaxToDescriptionWalkerConfiguration configuration) : BaseSyntaxVisitor
{
    private bool includeContext = configuration.IncludeContext;

    public Option<ModuleDescription> Context { get; private set; }

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

        var type = (NamedTypeDescription) base.VisitTypeDeclaration(node)! with { Members = VisitMembers() };

        if (includeContext)
        {
            includeContext = false;
            var walker = new ContextWalker();
            walker.Visit(node.Parent);
            Context = walker.GetRoot(type);
        }

        return type;
    }

    public override SyntaxDescription? VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        if (node.Modifiers.Any(SyntaxKind.PartialKeyword) is false)
        {
            return null;
        }

        return new MethodDescription
        {
            Modifiers = SyntaxHelper.ParseModifierList(node.Modifiers),
            ReturnType = (Visit(node.ReturnType) as TypeIdentifierDescription).ToOption(),
            Identifier = node.Identifier.ValueText,
            TypeParameterList = Visit(node.TypeParameterList) as TypeParameterListDescription ?? TypeParameterListDescription.Empty,
            Parameters = Visit(node.ParameterList) as ParameterListDescription ?? ParameterListDescription.Empty,
            Arity = node.Arity
        };
    }

    public override SyntaxDescription? VisitPropertyDeclaration(PropertyDeclarationSyntax node)
    {
        if (node.Modifiers.Any(SyntaxKind.PartialKeyword) is false)
        {
            return null;
        }

        return new PropertyDescription
        {
            Modifiers = SyntaxHelper.ParseModifierList(node.Modifiers),
            Type = (Visit(node.Type) as TypeIdentifierDescription).ToOption(),
            Identifier = node.Identifier.ValueText,
            Accessors = Visit(node.AccessorList) as AccessorListDescription ?? AccessorListDescription.Getter
        };
    }

    public override SyntaxDescription? VisitIndexerDeclaration(IndexerDeclarationSyntax node)
    {
        if (node.Modifiers.Any(SyntaxKind.PartialKeyword) is false)
        {
            return null;
        }

        return new IndexerDescription
        {
            Modifiers = SyntaxHelper.ParseModifierList(node.Modifiers),
            Type = (Visit(node.Type) as TypeIdentifierDescription).ToOption(),
            Parameters = Visit(node.ParameterList) as BracketedParameterListDescription ?? BracketedParameterListDescription.Empty,
            Accessors = Visit(node.AccessorList) as AccessorListDescription ?? AccessorListDescription.Getter
        };
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

internal class SyntaxDescriptionWalker<TResult> : SyntaxDescriptionVisitor<TResult> where TResult : notnull
{
    public override Option<TResult> VisitParameter(ParameterDescription description)
    {
        Visit(description.Modifiers);
        description.Type.Map(Visit);

        return None;
    }

    public override Option<TResult> VisitParameterList(ParameterListDescription description)
    {
        foreach (var parameter in description.Parameters)
        {
            Visit(parameter);
        }

        return None;
    }

    public override Option<TResult> VisitBracketedParameterList(BracketedParameterListDescription description)
    {
        foreach (var parameter in description.Parameters)
        {
            Visit(parameter);
        }

        return None;
    }

    public override Option<TResult> VisitMethod(MethodDescription description)
    {
        Visit(description.Modifiers);
        Visit(description.Parameters);
        Visit(description.TypeParameterList);
        description.ReturnType.Map(Visit);

        return None;
    }

    public override Option<TResult> VisitAccessorList(AccessorListDescription description)
    {
        foreach (var accessor in description.Accessors)
        {
            Visit(accessor);
        }

        return None;
    }

    public override Option<TResult> VisitProperty(PropertyDescription description)
    {
        Visit(description.Modifiers);
        Visit(description.Accessors);
        description.Type.Map(Visit);

        return None;
    }

    public override Option<TResult> VisitIndexer(IndexerDescription description)
    {
        Visit(description.Modifiers);
        Visit(description.Accessors);
        description.Type.Map(Visit);

        return None;
    }

    public override Option<TResult> VisitNamedType(NamedTypeDescription description)
    {
        Visit(description.Modifiers);
        Visit(description.TypeParameters);
        foreach (var member in description.Members)
        {
            Visit(member);
        }

        return None;
    }

    public override Option<TResult> VisitModule(ModuleDescription description)
    {
        foreach (var member in description.Members)
        {
            Visit(member);
        }

        return None;
    }

    public override Option<TResult> VisitNamespace(NamespaceDescription description)
    {
        foreach (var member in description.Members)
        {
            Visit(member);
        }

        return None;
    }
}

internal class SyntaxDescriptionEmitter(IEmitter emitter) : SyntaxDescriptionWalker<IEmitter>
{
    protected readonly IEmitter Emitter = emitter;

    public SyntaxDescriptionEmitter() : this(new StandardEmitter()) { }

    public override Option<IEmitter> VisitTypeIdentifier(TypeIdentifierDescription description)
    {
        return Emitter.Append(description.Identifier)
                      .Append(" ")
                      .Some();
    }

    public override Option<IEmitter> VisitModifierList(ModifierListDescription description)
    {
        Emitter.Tabs();
        foreach (var modifier in description.Modifiers)
        {
            Emitter.Append(modifier).Append(" ");
        }

        return Emitter.Some();
    }

    public override Option<IEmitter> VisitTypeParameterList(TypeParameterListDescription description)
    {
        if (description.Parameters.IsNone)
        {
            return None;
        }

        return Emitter
               // .Append("<")
               .Append(description.Parameters.UnsafeValue).Some();
        // .Append(">");
    }

    public override Option<IEmitter> VisitParameter(ParameterDescription description)
    {
        base.VisitParameter(description);
        return Emitter.Append(description.Identifier).Some();
    }

    public override Option<IEmitter> VisitParameterList(ParameterListDescription description)
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

        return Emitter.Append(")").Some();
    }

    public override Option<IEmitter> VisitBracketedParameterList(BracketedParameterListDescription description)
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

        return Emitter.Append("]").Some();
    }

    public virtual Option<IEmitter> VisitMethodBody(MethodDescription description)
    {
        return Emitter.NewLine()
                      .OpenBrace()
                      .CloseBrace()
                      .Some();
    }

    public override Option<IEmitter> VisitMethod(MethodDescription description)
    {
        Visit(description.Modifiers);
        Visit(description.ReturnType.DefaultValue(TypeIdentifierDescription.Error));
        Emitter.Append(description.Identifier);
        Visit(description.TypeParameterList);
        Visit(description.Parameters);
        VisitMethodBody(description);

        return Emitter.Some();
    }

    public override Option<IEmitter> VisitAccessor(AccessorDescription description) =>
        Emitter.Append(description.Keyword).Append(";").Some();

    public override Option<IEmitter> VisitAccessorList(AccessorListDescription description)
    {
        Emitter.Append("{ ");
        foreach (var accessor in description.Accessors)
        {
            Visit(accessor);
            Emitter.Append(" ");
        }

        return Emitter.Append("}").Some();
    }

    public override Option<IEmitter> VisitProperty(PropertyDescription description)
    {
        Visit(description.Modifiers);
        description.Type.Map(Visit);
        Emitter.Append(description.Identifier).Append(" ");
        return Visit(description.Accessors);
    }

    public override Option<IEmitter> VisitIndexer(IndexerDescription description)
    {
        Visit(description.Modifiers);
        description.Type.Map(Visit);
        Emitter.Append("this");
        Visit(description.Parameters);
        Emitter.Append(" ");
        return Visit(description.Accessors);
    }

    public virtual Option<IEmitter> VisitNamedTypeBody(NamedTypeDescription description)
    {
        return Emitter.Some();
    }

    public override Option<IEmitter> VisitNamedType(NamedTypeDescription description)
    {
        Visit(description.Modifiers);
        Emitter.Append(description.Keyword)
               .Append(" ")
               .Append(description.Identifier);
        Visit(description.TypeParameters);
        Emitter.NewLine()
               .OpenBrace();

        VisitNamedTypeBody(description);
        foreach (var member in description.Members)
        {
            Visit(member);
        }

        return Emitter.CloseBrace().Some();
    }

    public override Option<IEmitter> VisitNamespace(NamespaceDescription description)
    {
        Emitter.Tabs().Append("namespace ").Append(description.Name);
        _ = description.IsFileScoped switch
        {
            true  => Emitter.LineLine(";"),
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

        return Emitter.Some();
    }

    public override Option<IEmitter> VisitUsing(UsingDescription description) => Emitter.Line(description.Text).Some();

    public override Option<IEmitter> VisitModule(ModuleDescription description)
    {
        Emitter.NullableDirective();
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
            Emitter.NewLine();
        }

        return Emitter.Some();
    }

    public override string ToString() => Emitter.ToString();
}
