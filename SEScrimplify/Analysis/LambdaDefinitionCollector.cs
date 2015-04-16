using System;
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

        private ITypeSymbol TypeOf(SyntaxNode node)
        {
            if (node is ExpressionSyntax)
            {
                var typeInfo = semanticModel.GetTypeInfo(node);
                return typeInfo.Type;
            }
            if (node is BlockSyntax)
            {
                var block = (BlockSyntax)node;

                var returnTypes = block.Statements.OfType<ReturnStatementSyntax>()
                    .Select(r => semanticModel.GetTypeInfo(r.Expression).Type)
                    .Where(t => t != null)
                    .Distinct()
                    .ToList();

                if (returnTypes.Any()) return FindCommonType(returnTypes);
            }
            throw new NotSupportedException(node.GetType().FullName);
        }

        private ITypeSymbol FindCommonType(IList<ITypeSymbol> types)
        {
            if (types.Count == 1) return types.Single();

            throw new NotSupportedException("Multiple distinct types.");
        }

        public override void VisitIdentifierName(IdentifierNameSyntax node)
        {
            RecordMaybeCapturedIdentifier(node);
            base.VisitIdentifierName(node);

        }
        private void RecordMaybeCapturedIdentifier(IdentifierNameSyntax node)
        {
            if (CurrentLambda == null) return;
            var semanticInfo = semanticModel.GetSymbolInfo(node);
            if (semanticInfo.Symbol.Kind == SymbolKind.Parameter) return;
            if (semanticInfo.Symbol.Kind == SymbolKind.Method) return;
            if (semanticInfo.Symbol.Kind == SymbolKind.NamedType) return;
            CurrentLambda.AddDirectReference(semanticInfo.Symbol);
        }

        private void RecordMaybeNestedDeclaration(VariableDeclaratorSyntax node)
        {
            if (CurrentLambda == null) return;
            var symbol = semanticModel.GetDeclaredSymbol(node);
            if (symbol == null) return;
            CurrentLambda.AddDeclaration(symbol);
        }

        public override void VisitVariableDeclarator(VariableDeclaratorSyntax node)
        {
            RecordMaybeNestedDeclaration(node);
            base.VisitVariableDeclarator(node);
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