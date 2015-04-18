using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SEScrimplify.Rewrites.Lambda
{
    public class NoSymbolMapping : ISymbolMapping
    {
        public MemberAccessExpressionSyntax GetMappedMemberAccessExpression(ISymbol symbol)
        {
            return null;
        }
    }
}