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
            var expectedOutput = LoadScript("Rewrites.Lambda.NoExternalReferences.Rewritten.txt");

            var rewritten = RewriteScript(tree, new LambdaAsMemberFunctionRewrite(new GeneratedMemberNameProvider(0)));

            Assert.That(rewritten, Is.EqualTo(expectedOutput).Using<SyntaxTree>(new SyntaxEquivalenceComparer()));
        }

        [Test]
        public void CanRewriteLambdaWithMultipleArgumentsAsStaticMemberFunction()
        {
            var tree = LoadScript("Rewrites.Lambda.MultipleArguments.txt");
            var expectedOutput = LoadScript("Rewrites.Lambda.MultipleArguments.Rewritten.txt");

            var rewritten = RewriteScript(tree, new LambdaAsMemberFunctionRewrite(new GeneratedMemberNameProvider(0)));

            Assert.That(rewritten, Is.EqualTo(expectedOutput).Using<SyntaxTree>(new SyntaxEquivalenceComparer()));
        }

        [Test]
        public void CanRewriteLambdaReferencingMethodLocalVariableAsStructMemberFunction()
        {
            var tree = LoadScript("Rewrites.Lambda.ReferenceToMethodLocalVariable.txt");
            var expectedOutput = LoadScript("Rewrites.Lambda.ReferenceToMethodLocalVariable.Rewritten.txt");

            var rewritten = RewriteScript(tree, new LambdaAsMemberFunctionRewrite(new GeneratedMemberNameProvider(0)));
            
            Assert.That(rewritten, Is.EqualTo(expectedOutput).Using<SyntaxTree>(new SyntaxEquivalenceComparer()));
        }

        [Test]
        public void CanRewriteLambdaReferencingMemberOfClassAsStructMemberFunction()
        {
            var tree = LoadScript("Rewrites.Lambda.ReferenceToMemberOfClass.txt");
            var expectedOutput = LoadScript("Rewrites.Lambda.ReferenceToMemberOfClass.Rewritten.txt");

            var rewritten = RewriteScript(tree, new LambdaAsMemberFunctionRewrite(new GeneratedMemberNameProvider(0)));

            Assert.That(rewritten, Is.EqualTo(expectedOutput).Using<SyntaxTree>(new SyntaxEquivalenceComparer()));
        }
    }
}