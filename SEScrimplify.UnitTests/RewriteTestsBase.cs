using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace SEScrimplify.UnitTests
{
    public abstract class RewriteTestsBase
    {
        protected ScriptParser Parser = new ScriptParser();

        protected SyntaxTree LoadScript(string name)
        {
            var script = Parser.ParseScript(EmbeddedResources.GetScript(name));
            Parser.StrictCompile(script);
            return script;
        }

        protected SyntaxTree RewriteScript(SyntaxTree tree, ISemanticallyAwareRewrite rewrite)
        {
            var compilation = Parser.StrictCompile(tree);
            var rewrittenSyntax = SyntaxFactory.SyntaxTree(rewrite.Rewrite(tree.GetRoot(), Parser.GetSemanticModel(compilation, tree)).NormalizeWhitespace());
            Parser.StrictCompile(rewrittenSyntax);
            return rewrittenSyntax;
        }
    }
}