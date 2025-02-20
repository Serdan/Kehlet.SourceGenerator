using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PreludeTests;

enum TestEnum : ulong
{
    First = 1,
    Second = ulong.MaxValue
}

public class UnitTest1
{
    private const string ClassDeclaration = "public class MyType;";
    private const string StructDeclaration = "public struct MyType;";
    private const string RecordDeclaration = "public record MyType;";
    private const string RecordClassDeclaration = "public record class MyType;";
    private const string RecordStructDeclaration = "public record struct MyType;";
    private const string InterfaceDeclaration = "public interface MyType;";
    private const string TargetClass = "public static partial class MyType<T>;";

    [Fact]
    public void ImmutableArrayEqualityInOption()
    {
        var a = Some(CacheArray.Create(1, 2, 3, 4));
        var b = Some(CacheArray.Create(1, 2, 3, 4));

        Assert.Equal(a, b);
    }

    [Fact]
    public void ImmutableArrayEqualityInResult()
    {
        Result<CacheArray<int>, int> a = Ok(CacheArray.Create(1, 2, 3, 4));
        Result<CacheArray<int>, int> b = Ok(CacheArray.Create(1, 2, 3, 4));

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

    [Fact]
    public void DeclarationKind()
    {
        Assert.Equal("class", GetSyntaxNode(ClassDeclaration).GetKeyword());
        Assert.Equal("struct", GetSyntaxNode(StructDeclaration).GetKeyword());
        Assert.Equal("record", GetSyntaxNode(RecordDeclaration).GetKeyword());
        Assert.Equal("record class", GetSyntaxNode(RecordClassDeclaration).GetKeyword());
        Assert.Equal("record struct", GetSyntaxNode(RecordStructDeclaration).GetKeyword());
        Assert.Equal("interface", GetSyntaxNode(InterfaceDeclaration).GetKeyword());
    }

    private static TypeDeclarationSyntax GetSyntaxNode(string source) =>
        (TypeDeclarationSyntax) CSharpSyntaxTree.ParseText(source).GetRoot().ChildNodes().First();

    [Fact]
    public void GenericComparer()
    {
        var c = Equality<int>.ArrayComparer;
        var a = ImmutableArray.Create([1, 2, 3, 4]);
        var b = ImmutableArray.Create([1, 2, 3, 4]);

        Assert.True(c.Equals(a, b));
    }
}
