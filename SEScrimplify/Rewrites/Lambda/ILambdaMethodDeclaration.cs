using Microsoft.CodeAnalysis.CSharp.Syntax;
using SEScrimplify.Analysis;

namespace SEScrimplify.Rewrites.Lambda
{
    public interface ILambdaMethodDeclaration
    {
        LambdaModel Model { get; }

        ILambdaMethodDefinition DefineImplementation(BlockSyntax body);

        void CollectSymbolRewrites(RewriteList rewrites);

        /// <summary>
        /// Symbols remapped within this lambda, eg. because they've been replaced with fields.
        /// </summary>
        ISymbolMapping SymbolMapping { get; }
    }
}