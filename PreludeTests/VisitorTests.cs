using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PreludeTests;

public class VisitorTests
{
    private static SyntaxDescriptionEmitter GetEmitter() => new(new StandardEmitter());

    private const string Code =
        """
        using System;

        namespace Everything.Something;

        public partial class MyClass<T, U>
        {
            public partial int Main<T>(T value, string str) { return 0; }
            public partial int Value => 1;
            
            public partial int this[int index] => 2;
            
            public partial struct Test<A, B>
            {
                public partial class Innermost;
            }
        }
        """;

    [Fact]
    public Task TargetNodeIsType()
    {
        var tree = CSharpSyntaxTree.ParseText(Code);
        var target = (MemberDeclarationSyntax)tree.GetRoot().DescendantNodes().First(x => x is TypeDeclarationSyntax);

        var module = SyntaxHelper.GetTargetWithAll(target).UnsafeValue;

        var emitter = GetEmitter();
        emitter.Visit(module);
        var text = emitter.ToString();

        return Verify(text);
    }

    [Fact]
    public Task TargetNodeIsNestedType()
    {
        var tree = CSharpSyntaxTree.ParseText(Code);
        var target = (MemberDeclarationSyntax)tree.GetRoot().DescendantNodes().Last(x => x is TypeDeclarationSyntax);

        var module = SyntaxHelper.GetTargetWithAll(target).UnsafeValue;

        var emitter = GetEmitter();
        emitter.Visit(module);
        var text = emitter.ToString();

        return Verify(text);
    }

    [Fact]
    public Task TargetNodeIsMethod()
    {
        var tree = CSharpSyntaxTree.ParseText(Code);
        var target = (MemberDeclarationSyntax)tree.GetRoot().DescendantNodes().First(x => x is MethodDeclarationSyntax);

        var module = SyntaxHelper.GetTargetWithAll(target).UnsafeValue;

        var emitter = GetEmitter();
        emitter.Visit(module);
        var text = emitter.ToString();

        return Verify(text);
    }

    [Fact]
    public Task TargetNodeIsProperty()
    {
        var tree = CSharpSyntaxTree.ParseText(Code);
        var target = (MemberDeclarationSyntax)tree.GetRoot().DescendantNodes().First(x => x is PropertyDeclarationSyntax { Identifier.ValueText: "Value" });

        var module = SyntaxHelper.GetTargetWithAll(target).UnsafeValue;

        var emitter = GetEmitter();
        emitter.Visit(module);
        var text = emitter.ToString();

        return Verify(text);
    }

    [Fact]
    public Task TargetNodeIsIndexer()
    {
        var tree = CSharpSyntaxTree.ParseText(Code);
        var target = (MemberDeclarationSyntax)tree.GetRoot().DescendantNodes().First(x => x is IndexerDeclarationSyntax);

        var module = SyntaxHelper.GetTargetWithAll(target).UnsafeValue;

        var emitter = GetEmitter();
        emitter.Visit(module);
        var text = emitter.ToString();

        return Verify(text);
    }
}