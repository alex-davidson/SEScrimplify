using Microsoft.CodeAnalysis;

namespace SEScrimplify.Rewrites
{
    /// <summary>
    /// Walks the syntax tree identifying lambda functions, then rewrites the tree to
    /// use member functions of structs, etc instead.
    /// </summary>
    public class LambdaAsMemberFunctionRewrite : ISemanticallyAwareRewrite
    {
        public SyntaxNode Rewrite(SyntaxNode root, SemanticModel semanticModel)
        {
            return root;
        }

    }
}