using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SEScrimplify.Analysis
{
    public class LambdaDefinitionCollector : CSharpSyntaxWalker
    {
        private readonly List<LambdaModel> lambdas = new List<LambdaModel>();
        private readonly SemanticModel semanticModel;
        private readonly Stack<LambdaModel> stack = new Stack<LambdaModel>();
        private LambdaModel CurrentLambda { get { return stack.Count > 0 ? stack.Peek() : null; } }

        public LambdaDefinitionCollector(SemanticModel semanticModel)
        {
            this.semanticModel = semanticModel;
        }
        
        public IEnumerable<LambdaModel> GetModels()
        {
            return lambdas;
        }

        public override void VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
        {
            var lambda = new LambdaModel(node, node.Body, semanticModel.GetEffectiveTypeOf(node.Body), CurrentLambda, new[] { semanticModel.GetDeclaredSymbol(node.Parameter) });
            stack.Push(lambda);
            base.VisitSimpleLambdaExpression(node);
            var popped = stack.Pop();
            Debug.Assert(popped == lambda);

            lambdas.Add(lambda);
        }

        public override void VisitIdentifierName(IdentifierNameSyntax node)
        {
            RecordMaybeCapturedIdentifier(node);
            base.VisitIdentifierName(node);
        }

        public override void VisitVariableDeclarator(VariableDeclaratorSyntax node)
        {
            RecordMaybeNestedDeclaration(node);
            base.VisitVariableDeclarator(node);
        }

        public override void VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
        {
            var lambda = new LambdaModel(node, node.Body, semanticModel.GetEffectiveTypeOf(node.Body), CurrentLambda, node.ParameterList.Parameters.Select(p => semanticModel.GetDeclaredSymbol(p)).ToArray());
            stack.Push(lambda);
            base.VisitParenthesizedLambdaExpression(node);
            var popped = stack.Pop();
            Debug.Assert(popped == lambda);

            lambdas.Add(lambda);
        }

        private void RecordMaybeCapturedIdentifier(IdentifierNameSyntax node)
        {
            if (CurrentLambda == null) return;
            var semanticInfo = semanticModel.GetSymbolInfo(node);
            if (semanticInfo.Symbol.Kind == SymbolKind.Method) return;
            if (semanticInfo.Symbol.Kind == SymbolKind.NamedType) return;
            CurrentLambda.AddDirectReference(semanticInfo.Symbol, node);
        }

        private void RecordMaybeNestedDeclaration(VariableDeclaratorSyntax node)
        {
            if (CurrentLambda == null) return;
            var symbol = semanticModel.GetDeclaredSymbol(node);
            if (symbol == null) return;
            CurrentLambda.AddDeclaration(symbol);
        }
    }
}