using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SEScrimplify.Rewrites.Lambda
{
    public interface ILambdaMethodDefinition
    {
        ExpressionSyntax GetMethodExpression();

        MethodDeclarationSyntax GetMethodDeclaration();
    }
}