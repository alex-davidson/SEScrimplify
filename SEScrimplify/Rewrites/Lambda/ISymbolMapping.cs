using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SEScrimplify.Rewrites.Lambda
{
    public interface ISymbolMapping
    {
        MemberAccessExpressionSyntax GetMappedMemberAccessExpression(ISymbol symbol);
    }
}