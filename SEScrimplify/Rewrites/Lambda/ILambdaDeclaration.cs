using Microsoft.CodeAnalysis.CSharp.Syntax;
using SEScrimplify.Analysis;

namespace SEScrimplify.Rewrites.Lambda
{
    public interface ILambdaDeclaration
    {
        ILambdaMethodDefinition DefineLambda(BlockSyntax body);

        void AddSymbolRewrites(RewriteList rewrites);

        ISymbolMapping SymbolMapping { get; }
    }
}