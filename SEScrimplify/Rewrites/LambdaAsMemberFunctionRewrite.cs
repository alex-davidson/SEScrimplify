using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SEScrimplify.Analysis;
using SEScrimplify.Rewrites.Lambda;

namespace SEScrimplify.Rewrites
{
    /// <summary>
    /// Walks the syntax tree identifying lambda functions, then rewrites the tree to
    /// use member functions of structs, etc instead.
    /// </summary>
    public class LambdaAsMemberFunctionRewrite : ISemanticallyAwareRewrite
    {
        private readonly IGeneratedMemberNameProvider nameProvider;

        public LambdaAsMemberFunctionRewrite(IGeneratedMemberNameProvider nameProvider)
        {
            this.nameProvider = nameProvider;
        }

        public SyntaxNode Rewrite(SyntaxNode root, SemanticModel semanticModel)
        {
            var collector = new LambdaDefinitionCollector(semanticModel);
            collector.Visit(root);

            var lambdas = collector.GetDefinitions().ToArray();
            if (!lambdas.Any()) return root;

            var builder = new LambdaMethodsBuilder(nameProvider);

            var declarations = builder.ResolveContainingScopes(lambdas);

            var rewrites = new RewriteList();
            foreach (var lambda in lambdas)
            {
                var declaration = declarations[lambda];
                rewrites.Add(new RewriteAsMethodCall(lambda, declaration), lambda.SyntaxNode);
                declaration.AddSymbolRewrites(rewrites);
            }
            var rewritten = rewrites.ApplyRewrites(root);

            var scriptClass = rewritten.DescendantNodes().OfType<ClassDeclarationSyntax>().First();
            return rewritten.ReplaceNode(scriptClass, scriptClass.WithMembers(
                SyntaxFactory.List(scriptClass.Members.Concat(builder.GetTopLevelDeclarations()))));
        }

        class RewriteAsMethodCall : ISyntaxNodeRewrite
        {
            private readonly LambdaDefinition lambda;
            private readonly ILambdaDeclaration structDefinition;

            public RewriteAsMethodCall(LambdaDefinition lambda, ILambdaDeclaration structDefinition)
            {
                this.lambda = lambda;
                this.structDefinition = structDefinition;
            }

            public SyntaxNode Rewrite(SyntaxNode original, SyntaxNode current)
            {
                return structDefinition.DefineLambda(ConvertLambdaBodyToBlock(current)).GetMethodCallExpression();
            }

            private static BlockSyntax ConvertLambdaBodyToBlock(SyntaxNode lambdaSyntax)
            {
                if (lambdaSyntax is SimpleLambdaExpressionSyntax)
                {
                    return ((SimpleLambdaExpressionSyntax)lambdaSyntax).Body.ConvertLambdaBodyToBlock();
                }
                if(lambdaSyntax is ParenthesizedLambdaExpressionSyntax)
                {
                    return ((ParenthesizedLambdaExpressionSyntax)lambdaSyntax).Body.ConvertLambdaBodyToBlock();
                }
                throw new NotSupportedException(String.Format("Not a lambda? {0}", lambdaSyntax.GetType().Name));
            }
        }
    }
}