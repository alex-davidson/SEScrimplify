using Microsoft.CodeAnalysis;

namespace SEScrimplify
{
    public interface ISemanticallyAwareRewrite
    {
        SyntaxNode Rewrite(SyntaxNode root, SemanticModel semanticModel);
    }
}