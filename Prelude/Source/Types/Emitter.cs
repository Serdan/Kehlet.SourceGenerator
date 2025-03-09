using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;

namespace Kehlet.SourceGenerator;

internal interface IEmitter
{
    int CurrentIndent { get; set; }

    string TabString { get; }

    bool TabsPending { get; }

    /// <summary>
    /// Emit pending tabs.
    /// </summary>
    /// <returns></returns>
    IEmitter Tabs();

    /// <summary>
    /// Raw append. Ignores indent.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    IEmitter Append(string value);

    /// <summary>
    /// Emit newline
    /// </summary>
    /// <returns></returns>
    IEmitter NewLine();

    /// <summary>
    /// Converts the value of this instance to a <see cref="string"/>.
    /// </summary>
    /// <returns>A string whose value is the same as this instance.</returns>
    string ToString();

    /// <summary>
    /// Removes all characters from the current <see cref="IEmitter"/> instance.
    /// </summary>
    /// <returns>Empty <see cref="IEmitter"/> instance.</returns>
    IEmitter Clear();
}

internal abstract class Emitter : IEmitter
{
    protected int currentIndent;

    public const string DefaultTabString = "    ";

    public int CurrentIndent
    {
        get => currentIndent;
        set => currentIndent = Math.Max(0, value);
    }

    public virtual string TabString { get; } = DefaultTabString;

    public bool TabsPending { get; protected set; }

    public virtual IEmitter Tabs()
    {
        if (!TabsPending)
        {
            return this;
        }

        for (var i = 0; i < currentIndent; i++)
        {
            Append(TabString);
        }

        TabsPending = false;
        return this;
    }

    public abstract IEmitter Append(string value);
    public abstract IEmitter NewLine();
    public abstract IEmitter Clear();
    public abstract override string ToString();
}

/// <summary>
/// Automatically indents when appending. Doesn't indent empty lines.
/// <para/>
/// Uses a <see cref="StringBuilder"/> internally.
/// </summary>
/// <param name="builder"></param>
/// <param name="tabString"></param>
internal class StandardEmitter(StringBuilder builder, string tabString) : Emitter
{
    private readonly StringBuilder builder = builder;

    public override string TabString { get; } = tabString;

    public StandardEmitter() : this(new(), DefaultTabString)
    {
    }

    public StandardEmitter(string tabString) : this(new(), tabString)
    {
    }

    /// <inheritdoc />
    public override IEmitter Append(string value)
    {
        builder.Append(value);
        return this;
    }

    /// <inheritdoc />
    public override IEmitter NewLine()
    {
        builder.AppendLine();
        TabsPending = true;
        return this;
    }

    /// <inheritdoc />
    public override string ToString() =>
        builder.ToString();

    /// <inheritdoc />
    public override IEmitter Clear()
    {
        builder.Clear();
        return this;
    }
}

internal static class EmitterExtensions
{
    public static IEmitter Call(this IEmitter emitter, Func<IEmitter, IEmitter> f) => f(emitter);

    public static IEmitter WithIndent(this IEmitter emitter, int indent)
    {
        emitter.CurrentIndent = indent;
        return emitter;
    }

    public static IEmitter WithIndent(this IEmitter emitter, int indent, out int previousIndent)
    {
        previousIndent = emitter.CurrentIndent;
        emitter.CurrentIndent = indent;
        return emitter;
    }

    public static IEmitter Indent(this IEmitter emitter)
    {
        emitter.CurrentIndent++;
        return emitter;
    }

    public static IEmitter Unindent(this IEmitter emitter)
    {
        emitter.CurrentIndent--;
        return emitter;
    }

    /// <summary>
    /// Emit <paramref name="value"/> followed by newline.
    /// </summary>
    /// <param name="emitter"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static IEmitter Line(this IEmitter emitter, string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            emitter.Tabs().Append(value);
        }

        return emitter.NewLine();
    }

    /// <summary>
    /// Emit <paramref name="value"/> followed by two newlines.
    /// </summary>
    /// <param name="emitter"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static IEmitter LineLine(this IEmitter emitter, string value) => emitter.Line(value).NewLine();

    /// <summary>
    /// { -> newline -> indent
    /// </summary>
    /// <param name="emitter"></param>
    /// <returns></returns>
    public static IEmitter OpenBrace(this IEmitter emitter) => emitter.Line("{").Indent();

    /// <summary>
    /// unindent -> } -> newline
    /// </summary>
    /// <param name="emitter"></param>
    /// <returns></returns>
    public static IEmitter CloseBrace(this IEmitter emitter) => emitter.Unindent().Line("}");

    /// <summary>
    /// #nullable enable -> newline -> newline
    /// </summary>
    /// <param name="emitter"></param>
    /// <returns></returns>
    public static IEmitter NullableDirective(this IEmitter emitter) => emitter.LineLine("#nullable enable");

    /// <summary>
    /// Emit type.
    /// </summary>
    /// <param name="emitter"></param>
    /// <param name="typeEmitter"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public static IEmitter Type(this IEmitter emitter, ITypeEmitter typeEmitter, TypeBaseData data) =>
        emitter.Call(e => typeEmitter.TypeAttributes(e, data))
               .Line(data.GetTypeDeclaration())
               .OpenBrace()
               .Call(typeEmitter.TypeBody)
               .CloseBrace();

    /// <summary>
    /// Emit nested types.
    /// </summary>
    /// <param name="emitter"></param>
    /// <param name="typeEmitter"></param>
    /// <param name="innerType"></param>
    /// <param name="parents"></param>
    /// <returns></returns>
    public static IEmitter NestingTypes(this IEmitter emitter, ITypeEmitter typeEmitter, TypeBaseData innerType, ImmutableStack<TypeBaseData> parents)
    {
        if (parents.IsEmpty)
        {
            return emitter.Type(typeEmitter, innerType);
        }

        parents = parents.Pop(out var current);

        return emitter.Call(e => typeEmitter.ParentTypeAttribute(e, current))
                      .Line(current.GetTypeDeclaration())
                      .OpenBrace()
                      .NestingTypes(typeEmitter, innerType, parents)
                      .CloseBrace();
    }

    public static IEmitter FullType(this IEmitter emitter, ITypeEmitter typeEmitter, TypeFullData data) =>
        emitter.LineLine(data.Type.GetNamespaceDeclaration())
               .NestingTypes(typeEmitter, data.Type, [..data.Parents]);

    public static IEmitter File(this IEmitter emitter, ITypeEmitter typeEmitter, TypeFullData data) =>
        emitter.NullableDirective()
               .Call(e => typeEmitter.Usings(e, data))
               .FullType(typeEmitter, data);
}

internal interface ITypeEmitter
{
    IEmitter Usings(IEmitter emitter, TypeFullData data);
    public IEmitter TypeAttributes(IEmitter emitter, TypeBaseData typeBaseData);
    public IEmitter ParentTypeAttribute(IEmitter emitter, TypeBaseData typeBaseData);
    public IEmitter TypeBody(IEmitter emitter);
}

internal abstract class TypeEmitter : ITypeEmitter
{
    public virtual IEmitter Usings(IEmitter emitter, TypeFullData data) => emitter;
    public virtual IEmitter TypeAttributes(IEmitter emitter, TypeBaseData typeBaseData) => emitter;

    public virtual IEmitter ParentTypeAttribute(IEmitter emitter, TypeBaseData typeBaseData) => emitter;

    public virtual IEmitter TypeBody(IEmitter emitter) => emitter;
}

internal abstract class TypeVisitor
{
    public virtual IEmitter Emitter { get; } = new StandardEmitter();

    public virtual void EmitTypeFullData(TypeFullData data)
    {
        EmitFileStart();
        EmitUsings(data);
        EmitNamespace(data.Type);
        EmitTypeParent(data.Type, data.Parents, 0);
        EmitFileEnd();
    }

    public virtual void EmitFileStart() => Emitter.NullableDirective();

    public virtual void EmitFileEnd() { }

    public virtual void EmitUsings(TypeFullData data) { }

    public virtual void EmitNamespace(TypeData data) => Emitter.Line(data.GetNamespaceDeclaration());

    public virtual void EmitTypeParent(TypeBaseData innerType, CacheStack<TypeBaseData> parents, int depth)
    {
        Debug.Assert(depth < 100, "Please don't.");
        if (parents.IsEmpty)
        {
            EmitType(innerType);
            return;
        }

        parents = parents.Pop(out var current);
        EmitParentTypeAttributes(current, depth);
        EmitParentTypeDeclaration(current, depth);
        Emitter.OpenBrace();
        EmitParentTypeBody(current, depth);
        EmitTypeParent(innerType, parents, depth + 1);
        Emitter.CloseBrace();
    }

    public virtual void EmitParentTypeAttributes(TypeBaseData typeBaseData, int depth) { }

    public virtual void EmitParentTypeDeclaration(TypeBaseData typeBaseData, int depth) => Emitter.Line(typeBaseData.GetTypeDeclaration());

    public virtual void EmitParentTypeBody(TypeBaseData typeBaseData, int depth) { }

    public virtual void EmitType(TypeBaseData data)
    {
        EmitTypeAttributes(data);
        EmitTypeDeclaration(data);
        Emitter.OpenBrace();
        EmitTypeBody(data);
        Emitter.CloseBrace();
    }

    public virtual void EmitTypeAttributes(TypeBaseData typeBaseData) { }

    public virtual void EmitTypeDeclaration(TypeBaseData typeBaseData) => Emitter.Line(typeBaseData.GetTypeDeclaration());

    public virtual void EmitTypeBody(TypeBaseData typeBaseData) { }
}
