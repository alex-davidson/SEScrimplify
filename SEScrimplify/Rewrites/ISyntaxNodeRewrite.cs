using Microsoft.CodeAnalysis;

namespace SEScrimplify.Rewrites
{
    /// <summary>
    /// Declares a deferred rewrite of a SyntaxNode which may already have been rewritten at least once.
    /// </summary>
    public interface ISyntaxNodeRewrite
    {
        SyntaxNode Rewrite(SyntaxNode original, SyntaxNode current);
    }
}