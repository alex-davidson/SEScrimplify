using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SEScrimplify.Analysis
{
    public class LambdaDefinitionCollector : CSharpSyntaxWalker
    {
        public IEnumerable<LambdaDefinition> GetDefinitions()
        {
            return lambdas;
        }

        private readonly List<LambdaDefinition> lambdas = new List<LambdaDefinition>();
        private readonly SemanticModel semanticModel;

        public LambdaDefinitionCollector(SemanticModel semanticModel)
        {
            this.semanticModel = semanticModel;
        }

        private readonly Stack<LambdaDefinition> stack = new Stack<LambdaDefinition>();

        private LambdaDefinition CurrentLambda { get { return stack.Count > 0 ? stack.Peek() : null; } }

        public override void VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
        {
            var lambda = new LambdaDefinition(node, node.Body, TypeOf(node.Body), CurrentLambda, new[] { semanticModel.GetDeclaredSymbol(node.Parameter) });
            stack.Push(lambda);
            base.VisitSimpleLambdaExpression(node);
            var popped = stack.Pop();
            Debug.Assert(popped == lambda);

            lambdas.Add(lambda);
        }

        private TypeSyntax TypeOf(SyntaxNode node)
        {
            var typeInfo = semanticModel.GetTypeInfo(node);
            return SyntaxFactory.ParseTypeName(typeInfo.Type.ToDisplayString());
        }

        public override void VisitIdentifierName(IdentifierNameSyntax node)
        {
            SymbolInfo model;
            if(IsCapture(node, out model))
            {
                CurrentLambda.AddDirectReference(model.Symbol);
                //CurrentLambda.DirectReferences.Add(node, model.Symbol.DeclaringSyntaxReferences.
            }
            base.VisitIdentifierName(node);

        }
        private bool IsCapture(IdentifierNameSyntax node, out SymbolInfo semanticInfo)
        {
            semanticInfo = default(SymbolInfo);
            if (CurrentLambda == null) return false;
            var decl = semanticModel.GetDeclaredSymbol(node);
            semanticInfo = semanticModel.GetSymbolInfo(node);
            if (semanticInfo.Symbol.Kind == SymbolKind.Parameter) return false;
            if (semanticInfo.Symbol.Kind == SymbolKind.Method) return false;
            return true;
        }

        public override void VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
        {
            var lambda = new LambdaDefinition(node, node.Body, TypeOf(node.Body), CurrentLambda, node.ParameterList.Parameters.Select(p => semanticModel.GetDeclaredSymbol(p)).ToArray());
            stack.Push(lambda);
            base.VisitParenthesizedLambdaExpression(node);
            var popped = stack.Pop();
            Debug.Assert(popped == lambda);

            lambdas.Add(lambda);
        }

    }
}