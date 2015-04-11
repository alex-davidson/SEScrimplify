using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SEScrimplify.Analysis
{
    public class ExtensionMethodCallCollector : CSharpSyntaxWalker
    {
        public IEnumerable<ExtensionMethodCall> GetCalls()
        {
            return extensionMethodCalls;
        }

        private readonly List<ExtensionMethodCall> extensionMethodCalls = new List<ExtensionMethodCall>();
        private readonly SemanticModel semanticModel;

        public ExtensionMethodCallCollector(SemanticModel semanticModel)
        {
            this.semanticModel = semanticModel;
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            MaybeRewrite(node);
            base.VisitInvocationExpression(node);
        }

        private void MaybeRewrite(InvocationExpressionSyntax node)
        {
            var model = semanticModel.GetSymbolInfo(node);

            var method = model.Symbol;
            if (method == null) return;
            if (!method.ContainingType.MightContainExtensionMethods) return; // Probably not an extension method.
            if (method.IsStatic) return; // Already a static call.

            extensionMethodCalls.Add(new ExtensionMethodCall(node, method));
        }
    }
}