using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PreludeTests;

public class VisitorTests
{
    private readonly SyntaxToDescriptionWalker walker = new(SyntaxToDescriptionWalkerConfiguration.All);

    private int Number => 2;
    private int Number2 { get => 2; }
    private int Number3 { get { return 2;} }

    private const string Code =
        """
        using System;
        
        namespace Everything.Something
        {
            namespace Inner
            {
                using System;

                public partial class MyClass<T, U>
                {
                    public partial int Main<T>(T value, string str) { return 0; }
                    public partial int Value => 1;
                    
                    public partial int this[int index] => 2;
                    
                    public partial struct Test<A, B>;
                }
            }
        }
        """;

    [Fact]
    public void Test()
    {
        // var compilation = CSharpCompilation.Create(
        //     "Test",
        //     [CSharpSyntaxTree.ParseText(Code)],
        //     [MetadataReference.CreateFromFile(typeof(Object).Assembly.Location)],
        //     new(outputKind: OutputKind.DynamicallyLinkedLibrary)
        // );

        var tree = CSharpSyntaxTree.ParseText(Code);

        var root = (CSharpSyntaxNode) tree.GetRoot();
        var typeDeclaration = (TypeDeclarationSyntax) root.DescendantNodes().First(x => x is TypeDeclarationSyntax);
        var method = typeDeclaration.Members.First(x => x is MethodDeclarationSyntax);

        var module = SyntaxHelper.GetTargetWithContext(typeDeclaration).UnsafeValue;

        var emitter = new SyntaxDescriptionEmitter(new StandardEmitter());
        emitter.Visit(module);
        var test = emitter.ToString();
    }
}
