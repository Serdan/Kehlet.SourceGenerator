using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PreludeTests;

public class VisitorTests
{
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
    public void Test()
    {
        var tree = CSharpSyntaxTree.ParseText(Code);
        var typeDeclaration = (TypeDeclarationSyntax) tree.GetRoot().DescendantNodes().First(x => x is TypeDeclarationSyntax);

        var module = SyntaxHelper.GetTargetWithAll(typeDeclaration).UnsafeValue;

        var text = new SyntaxDescriptionEmitter(new StandardEmitter()).Visit(module).UnsafeValue.ToString();
    }
}
