using Microsoft.CodeAnalysis;

namespace SEScrimplify.Analysis
{
    public class ExtensionMethodCall
    {
        public ExtensionMethodCall(SyntaxNode syntaxNode, ISymbol actualMethod)
        {
            SyntaxNode = syntaxNode;
            ActualMethod = actualMethod;
        }

        public SyntaxNode SyntaxNode { get; private set; }
        public ISymbol ActualMethod { get; private set; }
    }
}