using NUnit.Framework;
using SEScrimplify.Rewrites;

namespace SEScrimplify.UnitTests.Rewrites
{
    public class RewriteTests : RewriteTestsBase
    {
        [TestCase("NoExternalReferences")]
        [TestCase("MultipleArguments")]
        [TestCase("MultipleLambdas")]
        [TestCase("ReferenceToMethodLocalVariable")]
        [TestCase("ReferenceToFieldOfClass")]
        [TestCase("ReferenceToPropertyOfClass")]
        [TestCase("MultipleScopes")]
        public void LambdaAsMemberFunctionRewrite(string scriptName)
        {
            ExpectEquivalenceAfterRewrite(new LambdaAsMemberFunctionRewrite(new GeneratedMemberNameProvider(0)), "Rewrites.Lambda", scriptName);
        }

        [TestCase("ExtensionCall", "StaticCall")]
        [TestCase("StaticCall", "StaticCall", Description = "Existing static calls should be left unchanged.")]
        public void ExtensionMethodCallAsStaticMethodCallRewrite(string originalScriptName, string rewrittenScriptName)
        {
            ExpectEquivalenceAfterRewrite(new ExtensionMethodCallAsStaticMethodCallRewrite(), "Rewrites.ExtensionMethod", originalScriptName, rewrittenScriptName);
        }
    }
}