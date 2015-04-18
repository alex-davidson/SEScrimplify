using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SEScrimplify.Rewrites.Lambda
{
    public class AvailableField
    {
        public SyntaxToken Name { get; private set; }
        public ITypeSymbol Type { get; private set; }
        public MemberDeclarationSyntax Declaration { get; private set; }           // public <type> <field>

        public MemberAccessExpressionSyntax CreateReference(ExpressionSyntax instance)      // instance.<field>
        {
            return SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, instance, SyntaxFactory.IdentifierName(Name));
        }

        public AvailableField(ITypeSymbol type, string name)
        {
            Name = SyntaxFactory.Identifier(name);
            Type = type;
            Declaration = SyntaxFactory.FieldDeclaration(
                SyntaxFactory.VariableDeclaration(
                    SyntaxFactory.ParseTypeName(type.ToDisplayString()),
                    SyntaxFactory.SeparatedList(new[] { SyntaxFactory.VariableDeclarator(Name) })))
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)));
        }
    }
}