using NUnit.Framework;
using SEScrimplify.Rewrites;

namespace SEScrimplify.UnitTests.Rewrites
{
    public class RewriteTests : RewriteTestsBase
    {
        [TestCase("NoExternalReferences")]
        [TestCase("BlockWithNoExternalReferences")]
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
            ExpectEquivalenceAfterRewrite(new BatchedRewrite(new ExtensionMethodCallAsStaticMethodCallRewrite()), "Rewrites.ExtensionMethod", originalScriptName, rewrittenScriptName);
        }


        [TestCase("ChildHasNoExternalReferences")]
        [TestCase("ChildReferencesOnlyTopScope")]
        [TestCase("ChildReferencesParentScope")]
        [TestCase("ChildReferencesParameterOfParentScope")]
        public void NestedLambdaAsMemberFunctionRewrite(string scriptName)
        {
            ExpectEquivalenceAfterRewrite(new LambdaAsMemberFunctionRewrite(new GeneratedMemberNameProvider(0)), "Rewrites.Lambda.Nested", scriptName);
        }
    }
}