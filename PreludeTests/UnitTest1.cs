using System.Collections.Immutable;

namespace PreludeTests;

enum TestEnum : ulong
{
    First = 1,
    Second = ulong.MaxValue
}

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

    [Fact]
    public void GetEnumMember()
    {
        var a = EnumHelper.GetMember<TestEnum, ulong>(1ul).UnsafeValue;
        
        Assert.Equal(TestEnum.First, a);
    }

    [Fact]
    public void ImplicitResultConversions()
    {
        Result<int, string> GetValue()
        {
            return 5;
        }
        
        
        Result<int, string> GetError()
        {
            return "error";
        }

        var value = GetValue();
        var error = GetError();
        
        Assert.Equal(Ok(5), value);
        Assert.Equal(Error("error"), error);
    }
}
