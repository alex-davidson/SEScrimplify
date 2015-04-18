using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SEScrimplify.Rewrites.Lambda
{
    public class ScopeMethodDefinition : ILambdaMethodDefinition
    {
        private readonly ScopeMethodDeclaration declaration;
        private readonly BlockSyntax body;
        
        public ScopeMethodDefinition(ScopeMethodDeclaration declaration, BlockSyntax body)
        {
            this.declaration = declaration;
            this.body = body;
        }

        public ExpressionSyntax GetMethodCallExpression()
        {
            return declaration.GetMethodCallExpression();
        }

        public MethodDeclarationSyntax GetMethodDeclaration()
        {
            return SyntaxFactory.MethodDeclaration(declaration.Model.GetReturnTypeSyntax(), declaration.MethodName.Identifier)
                .WithBody(body)
                .WithParameterList(declaration.Model.GetParameterListSyntax())
                .WithModifiers(SyntaxFactory.TokenList(
                    SyntaxFactory.Token(SyntaxKind.PublicKeyword)));
        }
    }
}