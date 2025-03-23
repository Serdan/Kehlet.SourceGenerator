namespace PreludeTests;

public class EmitterTests
{
    [Fact]
    public void EmitterRawStringLiteral()
    {
        var text = Emitter.NewEmitter.RawStringLiteral(""""" """" """"").ToString();

        const string expected =
            """"""
            """""
             """" 
            """"";

            """""";

        Assert.Equal(expected, text);
    }
}