using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SEScrimplify.Rewrites;

namespace SEScrimplify
{
    public class BatchedRewrite : IIndependentRewrite
    {
        private readonly ISemanticallyAwareRewrite[] rewriteDefinitions;

        public BatchedRewrite(params ISemanticallyAwareRewrite[] rewriteDefinitions)
        {
            this.rewriteDefinitions = rewriteDefinitions;
        }

        public SyntaxTree Rewrite(SyntaxTree tree, IScriptCompiler compiler)
        {
            var semanticModel = compiler.Compile(tree).GetSemanticModel(tree);
            
            var root = tree.GetRoot();
            var batch = new RewriteList();
            foreach (var rewrite in rewriteDefinitions)
            {
                rewrite.CollectRewrites(batch, root, semanticModel);
            }

            return SyntaxFactory.SyntaxTree(batch.ApplyRewrites(root));
        }
    }
}