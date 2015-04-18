using Microsoft.CodeAnalysis;
using SEScrimplify.Rewrites;

namespace SEScrimplify
{
    /// <summary>
    /// Plans and queues syntax rewrites based on the semantic model. These rewrites may
    /// be batched and applied alongside others.
    /// </summary>
    public interface ISemanticallyAwareRewrite
    {
        void CollectRewrites(RewriteList rewrites, SyntaxNode root, SemanticModel semanticModel);
    }
}