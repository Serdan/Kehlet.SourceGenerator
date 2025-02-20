using System.Collections.Immutable;

namespace PreludeTests;

public class UnitTest1
{
    [Fact]
    public void ImmutableArrayEqualityInOption()
    {
        var a = Some(ImmutableArray.Create([1, 2, 3, 4]));
        var b = Some(ImmutableArray.Create([1, 2, 3, 4]));

        Assert.Equal(a, b);
    }

    [Fact]
    public void ImmutableArrayEqualityInResult()
    {
        Result<ImmutableArray<int>, int> a = Ok(ImmutableArray.Create([1, 2, 3, 4]));
        Result<ImmutableArray<int>, int> b = Ok(ImmutableArray.Create([1, 2, 3, 4]));

        Assert.Equal(a, b);
    }
}
