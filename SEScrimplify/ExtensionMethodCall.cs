using Microsoft.CodeAnalysis;

namespace SEScrimplify
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