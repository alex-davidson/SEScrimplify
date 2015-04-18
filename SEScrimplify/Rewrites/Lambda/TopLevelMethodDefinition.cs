using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SEScrimplify.Analysis;

namespace SEScrimplify.Rewrites.Lambda
{
    class TopLevelMethodDefinition : ILambdaMethodDefinition
    {
        private readonly LambdaDefinition definition;
        private readonly BlockSyntax body;

        public TopLevelMethodDefinition(string name, LambdaDefinition definition, BlockSyntax body)
        {
            this.definition = definition;
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
            return SyntaxFactory.MethodDeclaration(definition.GetReturnTypeSyntax(), MethodName.Identifier)
                .WithBody(body)
                .WithParameterList(definition.GetParameterListSyntax())
                .WithModifiers(SyntaxFactory.TokenList(
                    SyntaxFactory.Token(SyntaxKind.StaticKeyword)));
        }
    }
}