using System.Text;

namespace Kehlet.SourceGenerator.Source;

internal class IndentedStringBuilder(StringBuilder builder, string tabString)
{
    private readonly StringBuilder builder = builder;
    private int indent;
    private bool tabsPending;
    private readonly string tabString = tabString;

    public const string DefaultTabString = "    ";

    public IndentedStringBuilder() : this(new(), DefaultTabString)
    {
    }

    public IndentedStringBuilder(string tabString) : this(new(), tabString)
    {
    }

    public int Indent
    {
        get => indent;
        set => indent = Math.Max(0, value);
    }

    private IndentedStringBuilder AppendTabs()
    {
        if (tabsPending)
        {
            for (var i = 0; i < indent; i++)
            {
                builder.Append(tabString);
            }

            tabsPending = false;
        }

        return this;
    }

    public IndentedStringBuilder Append(string value)
    {
        AppendTabs();
        builder.Append(value);
        return this;
    }

    public IndentedStringBuilder Append(string format, params object[] args)
    {
        AppendTabs();
        builder.AppendFormat(format, args);
        return this;
    }

    public IndentedStringBuilder AppendLine()
    {
        builder.AppendLine();
        tabsPending = true;
        return this;
    }

    public IndentedStringBuilder AppendLine(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            AppendLine();
        }
        else
        {
            AppendTabs();
            builder.AppendLine(value);
            tabsPending = true;
        }

        return this;
    }

    public IndentedStringBuilder AppendLine(string format, params object[] args)
    {
        AppendTabs();
        builder.AppendFormat(format, args);
        builder.AppendLine();
        tabsPending = true;
        return this;
    }
}
