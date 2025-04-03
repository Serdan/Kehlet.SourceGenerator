using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

using static Kehlet.SourceGenerator.Indent;

namespace Kehlet.SourceGenerator;

internal readonly struct Indent
{
    private Indent(int tag, int value)
    {
        this.tag = tag;
        this.value = value;
    }

    private readonly int tag;
    private readonly int value;

    public Emitter Apply(Emitter emitter) => tag switch
    {
        0 => emitter.WithIndent(emitter.CurrentIndent + value),
        1 => emitter.WithIndent(value),
        _ => throw new ArgumentOutOfRangeException($"Impossible DU state: {tag}")
    };

    public static Indent Delta(int value) => new(0, value);
    public static Indent Value(int value) => new(1, value);
    public static Indent Increment => Delta(1);
    public static Indent Decrement => Delta(-1);
}

internal readonly struct Scope
{
    private readonly int tag;
    private readonly int indent;

    private Scope(int tag, int indent)
    {
        this.tag = tag;
        this.indent = indent;
    }

    public Emitter Apply(Emitter emitter) => tag switch
    {
        0 => emitter.EnterScope(indent),
        1 => emitter.ExitScope(),
        _ => throw new ArgumentOutOfRangeException($"Impossible DU state: {tag}")
    };

    public static Scope Enter(int indent) => new(0, indent);
    public static Scope Exit() => new(1, 0);
}

internal class Emitter(StringBuilder builder)
{
    public Emitter() : this(new()) { }

    protected int currentIndent;
    private readonly Stack<int> scopes = new();

    public const string DefaultTabString = "    ";

    public static Emitter Create() => new();

    public int CurrentIndent
    {
        get => currentIndent;
        set
        {
            currentIndent = Math.Max(0, value);
            TabsPending = true;
        }
    }

    /// <summary>
    /// Increase indent when appending exactly "{". Decrease indent when appending exactly "}".
    /// </summary>
    public bool AutoIndentOnBrace { get; set; } = false;
    public Emitter WithAutoIndentOnBrace(bool value)
    {
        AutoIndentOnBrace = value;
        return this;
    }

    public virtual string TabString => DefaultTabString;

    public bool TabsPending { get; protected set; }

    public Emitter EnterScope(int indent)
    {
        scopes.Push(CurrentIndent);
        CurrentIndent = indent;
        return this;
    }

    public Emitter ExitScope()
    {
        if (scopes.Count is 0)
        {
            return this;
        }

        CurrentIndent = scopes.Pop();
        return this;
    }

    /// <summary>
    /// Emit pending tabs.
    /// </summary>
    /// <returns></returns>
    public virtual Emitter Tabs()
    {
        if (!TabsPending)
        {
            return this;
        }

        for (var i = 0; i < currentIndent; i++)
        {
            builder.Append(TabString);
        }

        TabsPending = false;
        return this;
    }

    /// <summary>
    /// Append pending tabs and value
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public virtual Emitter Append(string value)
    {
        if (AutoIndentOnBrace && value is "}")
        {
            CurrentIndent--;
        }

        Tabs();
        builder.Append(value);
        if (AutoIndentOnBrace && value is "{")
        {
            CurrentIndent++;
        }

        return this;
    }

    /// <summary>
    /// Emit newline
    /// </summary>
    /// <returns></returns>
    public virtual Emitter NewLine()
    {
        builder.AppendLine();
        TabsPending = true;
        return this;
    }

    /// <summary>
    /// Removes all characters from the current <see cref="Emitter"/> instance.
    /// </summary>
    /// <returns>Empty <see cref="Emitter"/> instance.</returns>
    public virtual Emitter Clear()
    {
        builder.Clear();
        return this;
    }

    public override string ToString() => builder.ToString();

    public static Emitter operator *(Emitter emitter, string text) => emitter.Append(text);
    public static Emitter operator *(Emitter emitter, Indent indent) => indent.Apply(emitter);
    public static Emitter operator *(Emitter emitter, Scope scope) => scope.Apply(emitter);
    public static Emitter operator *(Emitter emitter, Func<Emitter, Emitter> f) => emitter.Call(f);
    public static Emitter operator *(Emitter emitter, Func<Emitter, Unit> f) => emitter.Call(e => f(e).Apply(_ => e));
    public static Emitter operator /(Emitter emitter, string text) => emitter.NewLine() * text;
    public static Emitter operator /(Emitter emitter, Unit unit) => emitter.NewLine();
    public static Emitter operator /(Emitter emitter, Indent indent) => emitter.NewLine() * indent;
    public static Emitter operator /(Emitter emitter, Scope scope) => emitter.NewLine() * scope;
    public static Emitter operator /(Emitter emitter, Func<Emitter, Emitter> f) => emitter.NewLine() * f;
    public static Emitter operator /(Emitter emitter, Func<Emitter, Unit> f) => emitter.NewLine() * f;
    public static Emitter operator ++(Emitter emitter) => emitter.Indent();
    public static Emitter operator --(Emitter emitter) => emitter.Unindent();
    public static Unit operator >>(Emitter emitter, Unit _) => unit;
}

internal static class EmitterExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Emitter Call(this Emitter emitter, Func<Emitter, Emitter> f) => f(emitter);

    public static Emitter WithIndent(this Emitter emitter, int indent)
    {
        emitter.CurrentIndent = indent;
        return emitter;
    }

    public static Emitter WithIndent(this Emitter emitter, int indent, out int previousIndent)
    {
        previousIndent = emitter.CurrentIndent;
        emitter.CurrentIndent = indent;
        return emitter;
    }

    public static Emitter Indent(this Emitter emitter) => emitter * Increment;

    public static Emitter Unindent(this Emitter emitter) => emitter * Decrement;

    /// <summary>
    /// Emit <paramref name="value"/> followed by newline.
    /// </summary>
    /// <param name="emitter"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static Emitter Line(this Emitter emitter, string value) =>
        string.IsNullOrWhiteSpace(value) switch
        {
            true => emitter / unit,
            false => emitter * value / unit
        };

    /// <summary>
    /// Emit <paramref name="value"/> followed by two newlines.
    /// </summary>
    /// <param name="emitter"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static Emitter LineLine(this Emitter emitter, string value) => emitter * value / unit / unit;

    /// <summary>
    /// { -> newline -> indent
    /// </summary>
    /// <param name="emitter"></param>
    /// <returns></returns>
    public static Emitter OpenBrace(this Emitter emitter) => emitter * "{ " / Increment;

    /// <summary>
    /// unindent -> } -> newline
    /// </summary>
    /// <param name="emitter"></param>
    /// <returns></returns>
    public static Emitter CloseBrace(this Emitter emitter) => emitter * Decrement * "} " / unit;

    /// <summary>
    /// #nullable enable -> newline -> newline
    /// </summary>
    /// <param name="emitter"></param>
    /// <returns></returns>
    public static Emitter NullableDirective(this Emitter emitter) => emitter * "#nullable enable" / unit / unit;

    /// <summary>
    /// Emits <paramref name="content"/> as a C# raw string literal.
    /// Counts the quotation marks (") in <paramref name="content"/> and may cause performance issues for very large strings.
    /// </summary>
    /// <param name="emitter"></param>
    /// <param name="content">Text to emit.</param>
    /// <returns></returns>
    public static Emitter RawStringLiteral(this Emitter emitter, string content)
    {
        static int CountQuotes(string text)
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

        return emitter.RawStringLiteral(CountQuotes(content), content);
    }

    /// <summary>
    /// Emits <paramref name="content"/> as a C# raw string literal.
    /// </summary>
    /// <param name="emitter"></param>
    /// <param name="quotesCount">Number of quotation marks (") to use for the raw string literal.</param>
    /// <param name="content">Text to emit.</param>
    /// <returns></returns>
    public static Emitter RawStringLiteral(this Emitter emitter, int quotesCount, string content)
    {
        var quotes = new string('"', Math.Max(3, quotesCount));

        return emitter * Scope.Enter(0) * quotes / content / quotes * ";" / Scope.Exit();
    }
}