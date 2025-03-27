namespace PreludeTests;

public class EmitterTests
{
    [Fact]
    public void EmitterRawStringLiteral()
    {
        var text = Emitter.Create().RawStringLiteral(""""" """" """"").ToString();

        const string expected =
            """"""
            """""
             """" 
            """"";

            """""";

        Assert.Equal(expected, text);
    }

    [Fact]
    public void Dsl()
    {
        var emitter = Emitter.Create();

        var identifier = "int";
        var parameterList = "(int a)";
        var invocationList = "(a)";

        var result = emitter * "public static " * identifier * " New" * parameterList * " => new" * invocationList * ";" / unit;
        var expected = "public static int New(int a) => new(a);\r\n";

        Assert.Equal(expected, result.ToString());
    }

    [Fact]
    public void Dsl2()
    {
        var emitter = Emitter.Create();
        var quotes = "\"\"\"";

        var result = emitter
                   * Indent.Increment
                   * "public static string Value =>"
                   / Scope.Enter(0) * quotes / "some text" / quotes * ";" * Scope.Exit()
                   / "// test";

        var expected = """"
                public static string Value =>
            """
            some text
            """;
                // test
            """";

        Assert.Equal(expected, result.ToString());
    }
}