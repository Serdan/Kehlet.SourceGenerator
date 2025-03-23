using System;
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

    public static IEmitter NewEmitter => new StandardEmitter();

    public int CurrentIndent
    {
        get => currentIndent;
        set => currentIndent = Math.Max(0, value);
    }

    public virtual string TabString => DefaultTabString;

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
    /// Emits <paramref name="content"/> as a C# raw string literal.
    /// Counts the quotation marks (") in <paramref name="content"/> and may cause performance issues for very large strings.
    /// </summary>
    /// <param name="emitter"></param>
    /// <param name="content">Text to emit.</param>
    /// <returns></returns>
    public static IEmitter RawStringLiteral(this IEmitter emitter, string content)
    {
        int GetRawStringQuotes(string text)
        {
            var count = 0;
            var currentCount = 0;

            foreach (var character in text)
            {
                if (character is '"')
                {
                    currentCount++;
                }
                else
                {
                    currentCount = 1;
                }

                if (currentCount > count)
                {
                    count = currentCount;
                }
            }

            return Math.Max(3, count);
        }

        return emitter.RawStringLiteral(GetRawStringQuotes(content), content);
    }

    /// <summary>
    /// Emits <paramref name="content"/> as a C# raw string literal.
    /// </summary>
    /// <param name="emitter"></param>
    /// <param name="quotesCount">Number of quotation marks (") to use for the raw string literal.</param>
    /// <param name="content">Text to emit.</param>
    /// <returns></returns>
    public static IEmitter RawStringLiteral(this IEmitter emitter, int quotesCount, string content)
    {
        var quotes = new string('"', Math.Max(3, quotesCount));

        return emitter.WithIndent(0, out var indent)
                      .Line(quotes)
                      .Line(content)
                      .Line($"{quotes};")
                      .WithIndent(indent);
    }
}