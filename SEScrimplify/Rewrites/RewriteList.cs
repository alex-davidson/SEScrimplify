using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace SEScrimplify.Rewrites
{
    /// <summary>
    /// Records a rewrite implementation against each original node, then applies them all in a single operation.
    /// </summary>
    /// <remarks>
    /// Useful when doing syntax rewrites based on semantic information. If the rewrites are independent, multiple
    /// types of rewrite can all be generated based on one original tree and then all applied in one go.
    /// </remarks>
    public class RewriteList
    {
        readonly List<KeyValuePair<SyntaxNode, ISyntaxNodeRewrite>> rewrites = new List<KeyValuePair<SyntaxNode,ISyntaxNodeRewrite>>();

        public RewriteList Add(ISyntaxNodeRewrite rewriter, params SyntaxNode[] original)
        {
            rewrites.AddRange(original.Select(o => new KeyValuePair<SyntaxNode, ISyntaxNodeRewrite>(o, rewriter)));
            return this;
        }

        public SyntaxNode ApplyRewrites(SyntaxNode root)
        {
            var lookup = rewrites.GroupBy(r => r.Key, r => r.Value).ToDictionary(g => g.Key);

            return root.ReplaceNodes(lookup.Keys, (o, n) => lookup[o].Aggregate(n, (c, r) => r.Rewrite(o, c)));
        }
    }
}