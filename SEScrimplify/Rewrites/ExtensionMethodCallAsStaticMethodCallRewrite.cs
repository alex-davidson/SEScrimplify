using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SEScrimplify.Analysis;

namespace SEScrimplify.Rewrites
{
    /// <summary>
    /// Walks the syntax tree identifying extension method calls, then rewrites the tree to
    /// use static calls instead.
    /// </summary>
    public class ExtensionMethodCallAsStaticMethodCallRewrite : ISemanticallyAwareRewrite
    {
        public SyntaxNode Rewrite(SyntaxNode root, SemanticModel semanticModel)
        {
            var collector = new ExtensionMethodCallCollector(semanticModel);
            collector.Visit(root);

            var rewrites = new RewriteList();
            foreach (var callDescription in collector.GetCalls())
            {
                rewrites.Add(new RewriteAsStaticCall(callDescription), callDescription.SyntaxNode);
            }

            return rewrites.ApplyRewrites(root);
        }

        class RewriteAsStaticCall : ISyntaxNodeRewrite
        {
            private readonly ExtensionMethodCall callDescription;

            public RewriteAsStaticCall(ExtensionMethodCall callDescription)
            {
                this.callDescription = callDescription;
            }

            public SyntaxNode Rewrite(SyntaxNode original, SyntaxNode current)
            {
                var node = (InvocationExpressionSyntax)current;
                var methodAccess = (MemberAccessExpressionSyntax)node.Expression;
                var instance = methodAccess.Expression;

                var newArgList = node.ArgumentList.Arguments.Insert(0, SyntaxFactory.Argument(instance));

                Debug.Assert(callDescription.ActualMethod.ContainingSymbol.CanBeReferencedByName);
                var replacement = node
                    .WithArgumentList(
                        node.ArgumentList.Update(
                            node.ArgumentList.OpenParenToken,
                            newArgList,
                            node.ArgumentList.CloseParenToken))
                    .WithExpression(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.ParseTypeName(callDescription.ActualMethod.ContainingSymbol.ToDisplayString()),
                        methodAccess.Name));
                return replacement;
            }
        }
    }
}
