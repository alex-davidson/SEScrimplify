using Microsoft.CodeAnalysis;

namespace SEScrimplify.Analysis
{
    public class LambdaDefinition
    {
        public LambdaDefinition(SyntaxNode syntaxNode)
        {
            SyntaxNode = syntaxNode;
        }

        public SyntaxNode SyntaxNode { get; private set; }
    }
}