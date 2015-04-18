using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using SEScrimplify.Util;

namespace SEScrimplify.UnitTests.Rewrites
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
        protected void ExpectEquivalenceAfterRewrite(IIndependentRewrite rewrite, string scriptRoot, string scriptName)
        {
            ExpectEquivalenceAfterRewrite(rewrite, scriptRoot, scriptName, String.Format("{0}.Rewritten", scriptName));
        }

        protected void ExpectEquivalenceAfterRewrite(IIndependentRewrite rewrite, string scriptRoot, string originalScriptName, string rewrittenScriptName)
        {
            var tree = LoadScript(String.Format("{0}.{1}.txt", scriptRoot, originalScriptName));
            var expectedOutput = LoadScript(String.Format("{0}.{1}.txt", scriptRoot, rewrittenScriptName));

            var rewritten = RewriteScript(tree, rewrite);

            Assert.That(rewritten, Is.EqualTo(expectedOutput).Using<SyntaxTree>(new SyntaxEquivalenceComparer()));
        }

        protected SyntaxTree RewriteScript(SyntaxTree tree, IIndependentRewrite rewrite)
        {
            var rewrittenSyntax = SyntaxFactory.SyntaxTree(rewrite.Rewrite(tree, Parser).GetRoot().NormalizeWhitespace());
            Parser.StrictCompile(rewrittenSyntax);
            return rewrittenSyntax;
        }
    }
}