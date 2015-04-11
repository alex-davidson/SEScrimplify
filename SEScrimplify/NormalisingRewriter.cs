using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace SEScrimplify
{
    public class NormalisingRewriter : CSharpSyntaxRewriter
    {
        public override SyntaxToken VisitToken(SyntaxToken token)
        {
            var rewritten = base.VisitToken(token);
            if (rewritten.HasLeadingTrivia || rewritten.HasTrailingTrivia) return rewritten.NormalizeWhitespace();
            return rewritten;
        }
        public override SyntaxNode Visit(SyntaxNode node)
        {
            if (node == null) return null;
            var rewritten = base.Visit(node);
            if (rewritten.HasLeadingTrivia || rewritten.HasTrailingTrivia) return rewritten.NormalizeWhitespace();
            return rewritten;
        }
    }
}