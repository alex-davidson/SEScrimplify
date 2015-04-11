using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace SEScrimplify.Analysis
{
    public class LambdaDefinitionCollector : CSharpSyntaxWalker
    {
        public IEnumerable<LambdaDefinition> GetDefinitions()
        {
            return extensionMethodCalls;
        }

        private readonly List<LambdaDefinition> extensionMethodCalls = new List<LambdaDefinition>();
        private readonly SemanticModel semanticModel;

        public LambdaDefinitionCollector(SemanticModel semanticModel)
        {
            this.semanticModel = semanticModel;
        }

        public override void VisitSimpleLambdaExpression(Microsoft.CodeAnalysis.CSharp.Syntax.SimpleLambdaExpressionSyntax node)
        {
            base.VisitSimpleLambdaExpression(node);
        }
    }
}