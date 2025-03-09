using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// ReSharper disable InvalidXmlDocComment

namespace Kehlet.SourceGenerator;

internal static class SyntaxTarget
{
    /// <summary>
    /// <see cref="MemberDeclarationSyntax"/>: Virtually any declaration with accessiblity modifiers, including namespaces, types, methods and fields.
    /// </summary>
    /// <param name="node">The syntax node to test</param>
    /// <returns>True if match</returns>
    public static bool Member(SyntaxNode node, CancellationToken _) => node is MemberDeclarationSyntax;
    
    /// <summary>
    /// <see cref="NamespaceDeclarationSyntax"/>: Matches namespace
    /// </summary>
    /// <param name="node">The syntax node to test</param>
    /// <returns>True if match</returns>
    public static bool Namespace(SyntaxNode node, CancellationToken _) => node is NamespaceDeclarationSyntax;

    /// <summary>
    /// <see cref="DelegateDeclarationSyntax"/>: Matches delegate
    /// </summary>
    /// <param name="node">The syntax node to test</param>
    /// <returns>True if match</returns>
    public static bool Delegate(SyntaxNode node, CancellationToken _) => node is DelegateDeclarationSyntax;

    /// <summary>
    /// <see cref="BaseTypeDeclarationSyntax"/>: Matches all types, including enum.
    /// </summary>
    /// <param name="node">The syntax node to test</param>
    /// <returns>True if match</returns>
    public static bool BaseType(SyntaxNode node, CancellationToken _) => node is BaseTypeDeclarationSyntax;

    /// <summary>
    /// <see cref="EnumDeclarationSyntax"/>: Matches any enum. Subclass of <see cref="BaseTypeDeclarationSyntax"/>
    /// </summary>
    /// <param name="node">The syntax node to test</param>
    /// <returns>True if match</returns>
    public static bool Enum(SyntaxNode node, CancellationToken _) => node is EnumDeclarationSyntax;

    /// <summary>
    /// <see cref="TypeDeclarationSyntax"/>: Matches any type, excluding enum. Includes interfaces, classes, structs and records.
    /// </summary>
    /// <param name="node">The syntax node to test</param>
    /// <returns>True if match</returns>
    public static bool Type(SyntaxNode node, CancellationToken _) => node is TypeDeclarationSyntax;

    /// <summary>
    /// <see cref="InterfaceDeclarationSyntax"/>: Matches any interface. Subclass of <see cref="TypeDeclarationSyntax"/>
    /// </summary>
    /// <param name="node">The syntax node to test</param>
    /// <returns>True if match</returns>
    public static bool Interface(SyntaxNode node, CancellationToken _) => node is InterfaceDeclarationSyntax;

    /// <summary>
    /// <see cref="ClassDeclarationSyntax"/>: Matches any class. Subclass of <see cref="TypeDeclarationSyntax"/>
    /// </summary>
    /// <param name="node">The syntax node to test</param>
    /// <returns>True if match</returns>
    public static bool Class(SyntaxNode node, CancellationToken _) => node is ClassDeclarationSyntax;

    /// <summary>
    /// <see cref="StructDeclarationSyntax"/>: Matches any struct. Subclass of <see cref="TypeDeclarationSyntax"/>
    /// </summary>
    /// <param name="node">The syntax node to test</param>
    /// <returns>True if match</returns>
    public static bool Struct(SyntaxNode node, CancellationToken _) => node is StructDeclarationSyntax;

    /// <summary>
    /// <see cref="RecordDeclarationSyntax"/>: Matches any record. Subclass of <see cref="TypeDeclarationSyntax"/>
    /// </summary>
    /// <param name="node">The syntax node to test</param>
    /// <returns>True if match</returns>
    public static bool Record(SyntaxNode node, CancellationToken _) => node is RecordDeclarationSyntax;

    /// <summary>
    /// <see cref="FieldDeclarationSyntax"/>: Matches any field.
    /// </summary>
    /// <param name="node">The syntax node to test</param>
    /// <returns>True if match</returns>
    public static bool Field(SyntaxNode node, CancellationToken _) => node is FieldDeclarationSyntax;

    /// <summary>
    /// <see cref="AccessorDeclarationSyntax"/>: Matches any accessor.
    /// </summary>
    /// <param name="node">The syntax node to test</param>
    /// <returns>True if match</returns>
    public static bool Accessor(SyntaxNode node, CancellationToken _) => node is AccessorDeclarationSyntax;

    /// <summary>
    /// <see cref="BaseMethodDeclarationSyntax"/>: Matches any method-like member. Includes constructors, destructors, methods and operators.
    /// </summary>
    /// <param name="node">The syntax node to test</param>
    /// <returns>True if match</returns>
    public static bool BaseMethod(SyntaxNode node, CancellationToken _) => node is BaseMethodDeclarationSyntax;

    /// <summary>
    /// <see cref="ConstructorDeclarationSyntax"/>: Matches any constructor. Subclass of <see cref="BaseMethodDeclarationSyntax"/>
    /// </summary>
    /// <param name="node">The syntax node to test</param>
    /// <returns>True if match</returns>
    public static bool Constructor(SyntaxNode node, CancellationToken _) => node is ConstructorDeclarationSyntax;

    /// <summary>
    /// <see cref="DestructorDeclarationSyntax"/>: Matches any destructor. Subclass of <see cref="BaseMethodDeclarationSyntax"/>
    /// </summary>
    /// <param name="node">The syntax node to test</param>
    /// <returns>True if match</returns>
    public static bool Destructor(SyntaxNode node, CancellationToken _) => node is DestructorDeclarationSyntax;

    /// <summary>
    /// <see cref="MethodDeclarationSyntax"/>: Matches any method. Subclass of <see cref="BaseMethodDeclarationSyntax"/>
    /// </summary>
    /// <param name="node">The syntax node to test</param>
    /// <returns>True if match</returns>
    public static bool Method(SyntaxNode node, CancellationToken _) => node is MethodDeclarationSyntax;

    /// <summary>
    /// <see cref="OperatorDeclarationSyntax"/>: Matches any operator. Subclass of <see cref="BaseMethodDeclarationSyntax"/>
    /// </summary>
    /// <param name="node">The syntax node to test</param>
    /// <returns>True if match</returns>
    public static bool Operator(SyntaxNode node, CancellationToken _) => node is OperatorDeclarationSyntax;

    /// <summary>
    /// <see cref="BasePropertyDeclarationSyntax"/>: Matches any property-like member. Includes properties, events and indexers.
    /// </summary>
    /// <param name="node">The syntax node to test</param>
    /// <returns>True if match</returns>
    public static bool BaseProperty(SyntaxNode node, CancellationToken _) => node is BasePropertyDeclarationSyntax;

    /// <summary>
    /// <see cref="PropertyDeclarationSyntax"/>: Matches any property. Subclass of <see cref="BasePropertyDeclarationSyntax"/>
    /// </summary>
    /// <param name="node">The syntax node to test</param>
    /// <returns>True if match</returns>
    public static bool Property(SyntaxNode node, CancellationToken _) => node is PropertyDeclarationSyntax;

    /// <summary>
    /// <see cref="EventDeclarationSyntax"/>: Matches any event. Subclass of <see cref="BasePropertyDeclarationSyntax"/>
    /// </summary>
    /// <param name="node">The syntax node to test</param>
    /// <returns>True if match</returns>
    public static bool Event(SyntaxNode node, CancellationToken _) => node is EventDeclarationSyntax;

    /// <summary>
    /// <see cref="IndexerDeclarationSyntax"/>: Matches any indexer. Subclass of <see cref="BasePropertyDeclarationSyntax"/>
    /// </summary>
    /// <param name="node">The syntax node to test</param>
    /// <returns>True if match</returns>
    public static bool Indexer(SyntaxNode node, CancellationToken _) => node is IndexerDeclarationSyntax;
}
