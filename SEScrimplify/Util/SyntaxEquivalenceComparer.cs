using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace SEScrimplify.Util
{
    public class SyntaxEquivalenceComparer : IEqualityComparer<SyntaxNode>, IEqualityComparer<SyntaxTree>
    {
        private static SourceText Normalise(SyntaxNode t)
        {
            return new NormalisingRewriter().Visit(t).GetText();
        }

        public bool Equals(SyntaxNode x, SyntaxNode y)
        {
            var nX = Normalise(x);
            var nY = Normalise(y);
            return nX.ContentEquals(nY);
        }

        public int GetHashCode(SyntaxNode obj)
        {
            return Normalise(obj).GetHashCode();
        }

        public bool Equals(SyntaxTree x, SyntaxTree y)
        {
            return Equals(x.GetRoot(), y.GetRoot());
        }

        public int GetHashCode(SyntaxTree obj)
        {
            return GetHashCode(obj.GetRoot());
        }
    }
}