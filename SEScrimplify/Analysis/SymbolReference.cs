using Microsoft.CodeAnalysis;

namespace SEScrimplify.Analysis
{
    public struct SymbolReference
    {
        public SymbolReference(ISymbol symbol, SyntaxNode syntaxNode) : this()
        {
            Symbol = symbol;
            SyntaxNode = syntaxNode;
        }

        public ISymbol Symbol { get; private set; }
        public SyntaxNode SyntaxNode { get; private set; }
    }
}