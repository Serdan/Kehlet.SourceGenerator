using System.Text;

namespace Kehlet.SourceGenerator.Source;

public class IndentedStringBuilder(StringBuilder builder, string tabString)
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

    private void AppendTabs()
    {
        if (tabsPending)
        {
            for (var i = 0; i < indent; i++)
            {
                builder.Append(tabString);
            }

            tabsPending = false;
        }
    }

    public void Append(string value)
    {
        AppendTabs();
        builder.Append(value);
    }

    public void Append(string format, params object[] args)
    {
        AppendTabs();
        builder.AppendFormat(format, args);
    }

    public void AppendLine()
    {
        builder.AppendLine();
        tabsPending = true;
    }

    public void AppendLine(string value)
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
    }

    public void AppendLine(string format, params object[] args)
    {
        AppendTabs();
        builder.AppendFormat(format, args);
        builder.AppendLine();
        tabsPending = true;
    }
}
