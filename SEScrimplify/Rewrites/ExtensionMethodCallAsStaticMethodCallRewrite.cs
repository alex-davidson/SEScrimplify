using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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

            var replacements = collector.GetCalls().ToDictionary(e => e.SyntaxNode);

            return root.ReplaceNodes(replacements.Keys, CreateRewriter(replacements));
        }

        private Func<SyntaxNode, SyntaxNode, SyntaxNode> CreateRewriter(Dictionary<SyntaxNode, ExtensionMethodCall> extensionMethodCalls)
        {
            return (o, n) =>
            {
                var call = extensionMethodCalls[o];
                return RewriteAsStaticCall(call, n);
            };
        }

        private SyntaxNode RewriteAsStaticCall(ExtensionMethodCall callDescription, SyntaxNode currentNode)
        {
            var node = (InvocationExpressionSyntax)currentNode;
            var methodAccess = (MemberAccessExpressionSyntax)node.Expression;
            var instance = methodAccess.Expression;

            var newArgList = node.ArgumentList.Arguments.Insert(0, SyntaxFactory.Argument(instance));

            var replacement = node
                .WithArgumentList(
                    node.ArgumentList.Update(
                        node.ArgumentList.OpenParenToken,
                        newArgList,
                        node.ArgumentList.CloseParenToken))
                .WithExpression(SyntaxFactory.IdentifierName(callDescription.ActualMethod.ContainingSymbol.ToDisplayString() + "." + methodAccess.Name));
            return replacement;
        }
    }
}
