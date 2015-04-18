using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SEScrimplify.Analysis;

namespace SEScrimplify.Rewrites.Lambda
{
    class ScopeMethodDefinition : ILambdaMethodDefinition
    {
        private readonly ScopeStructDefinition owner;
        private readonly LambdaDefinition definition;
        private readonly BlockSyntax body;
        private readonly IDictionary<ISymbol, AvailableField> symbolMappings;

        public ScopeMethodDefinition(string name, ScopeStructDefinition owner, LambdaDefinition definition, BlockSyntax body, IDictionary<ISymbol, AvailableField> symbolMappings)
        {
            this.owner = owner;
            this.definition = definition;
            this.body = body;
            this.symbolMappings = symbolMappings;
            MethodName = SyntaxFactory.IdentifierName(name);
        }

        public IdentifierNameSyntax MethodName { get; private set; }

        public ExpressionSyntax GetMethodCallExpression()
        {
            return SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, owner.GetCreationExpression(symbolMappings), MethodName);
        }

        public MethodDeclarationSyntax GetMethodDeclaration()
        {
            return SyntaxFactory.MethodDeclaration(definition.GetReturnTypeSyntax(), MethodName.Identifier)
                .WithBody(body)
                .WithParameterList(definition.GetParameterListSyntax())
                .WithModifiers(SyntaxFactory.TokenList(
                    SyntaxFactory.Token(SyntaxKind.PublicKeyword)));
        }
    }
}