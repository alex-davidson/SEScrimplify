using Microsoft.CodeAnalysis;
using NUnit.Framework;
using SEScrimplify.Rewrites;
using SEScrimplify.Util;

namespace SEScrimplify.UnitTests.Rewrites.ExtensionMethod
{
    [TestFixture]
    public class ExtensionMethodCallAsStaticMethodCallRewriteTests : RewriteTestsBase
    {
        [Test]
        public void CanRewriteExtensionMethodCallAsStaticMethodCall()
        {
            var tree = LoadScript("Rewrites.ExtensionMethod.ExtensionCall.txt");
            var expectedOutput = LoadScript("Rewrites.ExtensionMethod.StaticCall.txt");

            var rewritten = RewriteScript(tree,  new ExtensionMethodCallAsStaticMethodCallRewrite());

            Assert.That(rewritten, Is.EqualTo(expectedOutput).Using<SyntaxTree>(new SyntaxEquivalenceComparer()));
        }

        [Test]
        public void IgnoresStaticCallOfExtensionMethod()
        {
            var tree = LoadScript("Rewrites.ExtensionMethod.StaticCall.txt");
            
            var rewritten = RewriteScript(tree, new ExtensionMethodCallAsStaticMethodCallRewrite());

            Assert.That(rewritten, Is.EqualTo(tree).Using<SyntaxTree>(new SyntaxEquivalenceComparer()));
        }
    }
}