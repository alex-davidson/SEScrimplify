using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SEScrimplify
{
    public static class Extensions
    {
        public static BlockSyntax ConvertLambdaBodyToBlock(this CSharpSyntaxNode lambdaBody)
        {
            if (lambdaBody is BlockSyntax) return (BlockSyntax)lambdaBody;
            if (lambdaBody is ExpressionSyntax) return SyntaxFactory.Block(SyntaxFactory.ReturnStatement((ExpressionSyntax)lambdaBody));
            throw new NotSupportedException(lambdaBody.GetType().FullName);
        }

        public static IEnumerable<ExpressionSyntax> GetResultExpressions(this CSharpSyntaxNode lambdaBody)
        {
            if (lambdaBody is ExpressionSyntax) return new[] { (ExpressionSyntax)lambdaBody };
            if (lambdaBody is BlockSyntax) return lambdaBody.DescendantNodes().OfType<ReturnStatementSyntax>().Select(r => r.Expression);
            throw new NotSupportedException(lambdaBody.GetType().FullName);
        }

        public static ITypeSymbol GetEffectiveTypeOf(this SemanticModel semanticModel, CSharpSyntaxNode node)
        {
            var returnTypes = node.GetResultExpressions()
                .Select(r => semanticModel.GetTypeInfo(r).Type)
                .Where(t => t != null)
                .Distinct()
                .ToList();

            if (returnTypes.Any()) return FindCommonType(returnTypes);
            throw new NotSupportedException(node.GetType().FullName);
        }

        private static ITypeSymbol FindCommonType(IList<ITypeSymbol> types)
        {
            if (types.Count == 1) return types.Single();

            // Not yet implemented: find best common type.
            throw new NotSupportedException("Multiple distinct types.");
        }

        public static ITypeSymbol GetSymbolType(this ISymbol symbol)
        {
            if (symbol is IFieldSymbol) return ((IFieldSymbol)symbol).Type;
            if (symbol is IPropertySymbol) return ((IPropertySymbol)symbol).Type;
            if (symbol is ILocalSymbol) return ((ILocalSymbol)symbol).Type;
            if (symbol is IParameterSymbol) return ((IParameterSymbol)symbol).Type;
            return null;
        }

        public static TypeSyntax AsSyntax(this ITypeSymbol typeSymbol)
        {
            return SyntaxFactory.ParseTypeName(typeSymbol.ToDisplayString());
        }


        public static BlockSyntax PrependStatement(this StatementSyntax maybeBlock, StatementSyntax prepend)
        {
            var statements = new List<StatementSyntax> { prepend };
            if (maybeBlock is BlockSyntax)
            {
                statements.AddRange(((BlockSyntax)maybeBlock).Statements);
            }
            return SyntaxFactory.Block(SyntaxFactory.List(statements));
        }
    }
}