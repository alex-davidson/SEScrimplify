using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SEScrimplify.Analysis;

namespace SEScrimplify.Rewrites.Lambda
{
    class TopLevelMethodDefinition : ILambdaMethodDefinition
    {
        private readonly LambdaModel model;
        private readonly BlockSyntax body;

        public TopLevelMethodDefinition(string name, LambdaModel model, BlockSyntax body)
        {
            this.model = model;
            this.body = body;
            MethodName = SyntaxFactory.IdentifierName(name);
        }

        public IdentifierNameSyntax MethodName { get; private set; }

        public ExpressionSyntax GetMethodCallExpression()
        {
            return MethodName;
        }

        public MemberDeclarationSyntax GetTopLevelDeclaration()
        {
            // Static methods in the top-level class/scope don't need to be public, for some reason.
            return SyntaxFactory.MethodDeclaration(model.GetReturnTypeSyntax(), MethodName.Identifier)
                .WithBody(body)
                .WithParameterList(model.GetParameterListSyntax())
                .WithModifiers(SyntaxFactory.TokenList(
                    SyntaxFactory.Token(SyntaxKind.StaticKeyword)));
        }
    }
}