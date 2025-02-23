using System.Text;

namespace Kehlet.SourceGenerator.Source;

internal interface IEmitter
{
    int Indent { get; set; }

    IEmitter WithIndent(int indent);
    IEmitter WithIndent(int indent, out int previousIndent);

    /// <summary>
    /// Emits value
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    IEmitter Emit(string value);

    /// <summary>
    /// Emits newline
    /// </summary>
    /// <returns></returns>
    IEmitter Line();

    /// <summary>
    /// Emits value -> newline
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    IEmitter Line(string value);

    /// <summary>
    /// Emits value -> newline -> newline 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    IEmitter LineLine(string value);

    string ToString();
}

/// <summary>
/// Automatically indents when appending. Doesn't indent empty lines.
/// <para/>
/// Uses a <see cref="StringBuilder"/> internally.
/// </summary>
/// <param name="builder"></param>
/// <param name="tabString"></param>
internal sealed class StandardEmitter(StringBuilder builder, string tabString) : IEmitter
{
    private readonly StringBuilder builder = builder;
    private int indent;
    private bool tabsPending;
    private readonly string tabString = tabString;

    public const string DefaultTabString = "    ";

    public StandardEmitter() : this(new(), DefaultTabString)
    {
    }

    public StandardEmitter(string tabString) : this(new(), tabString)
    {
    }

    public int Indent
    {
        get => indent;
        set => indent = Math.Max(0, value);
    }

    public IEmitter WithIndent(int indent)
    {
        Indent = indent;
        return this;
    }

    public IEmitter WithIndent(int indent, out int previousIndent)
    {
        previousIndent = indent;
        Indent = indent;
        return this;
    }

    private void Tabs()
    {
        if (!tabsPending)
        {
            return;
        }

        for (var i = 0; i < indent; i++)
        {
            builder.Append(tabString);
        }

        tabsPending = false;
    }

    public IEmitter Emit(string value)
    {
        Tabs();
        builder.Append(value);
        return this;
    }

    public IEmitter Line()
    {
        builder.AppendLine();
        tabsPending = true;
        return this;
    }

    public IEmitter Line(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            Line();
        }
        else
        {
            Tabs();
            builder.AppendLine(value);
            tabsPending = true;
        }

        return this;
    }

    public IEmitter LineLine(string value) =>
        Line(value).Line();

    public override string ToString() =>
        builder.ToString();
}

internal static class EmitterExtensions
{
    /// <summary>
    /// { -> newline -> indent++
    /// </summary>
    /// <param name="emitter"></param>
    /// <returns></returns>
    public static IEmitter OpenBrace(this IEmitter emitter)
    {
        emitter.Line("{");
        emitter.Indent++;

        return emitter;
    }

    /// <summary>
    /// indent-- -> } -> newline
    /// </summary>
    /// <param name="emitter"></param>
    /// <returns></returns>
    public static IEmitter CloseBrace(this IEmitter emitter)
    {
        emitter.Indent--;
        emitter.Line("}");

        return emitter;
    }

    /// <summary>
    /// #nullable enable -> newline -> newline
    /// </summary>
    /// <param name="emitter"></param>
    /// <returns></returns>
    public static IEmitter NullableDirective(this IEmitter emitter)
    {
        emitter.LineLine("#nullable enable");

        return emitter;
    }
}
