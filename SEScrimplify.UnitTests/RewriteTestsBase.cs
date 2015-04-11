using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace SEScrimplify.UnitTests
{
    public abstract class RewriteTestsBase
    {
        protected ScriptParser Parser = new ScriptParser();

        protected SyntaxTree LoadScript(string name)
        {
            return Parser.ParseScript(EmbeddedResources.GetScript(name));
        }

        protected SyntaxTree RewriteScript(SyntaxTree tree, ISemanticallyAwareRewrite rewrite)
        {
            var compilation = Parser.StrictCompile(tree);
            return SyntaxFactory.SyntaxTree(rewrite.Rewrite(tree.GetRoot(), Parser.GetSemanticModel(compilation, tree)));
        }
    }
}