using Microsoft.CodeAnalysis;
using NUnit.Framework;
using SEScrimplify.Rewrites;
using SEScrimplify.Util;

namespace SEScrimplify.UnitTests.Rewrites.Lambda
{
    [TestFixture]
    public class LambdaAsMemberFunctionRewriteTests : RewriteTestsBase
    {
        [Test]
        public void CanRewriteLambdaWithNoExternalReferencesAsStaticMemberFunction()
        {
            var tree = LoadScript("Rewrites.Lambda.NoExternalReferences.txt");
            var expectedOutput = LoadScript("Rewrites.Lambda.NoExternalReferences.MemberFunction.txt");

            var rewritten = RewriteScript(tree, new LambdaAsMemberFunctionRewrite());

            Assert.That(rewritten, Is.EqualTo(expectedOutput).Using<SyntaxTree>(new SyntaxEquivalenceComparer()));
        }
    }
}