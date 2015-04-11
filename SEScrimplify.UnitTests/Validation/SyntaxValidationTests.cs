using System.Linq;
using NUnit.Framework;

namespace SEScrimplify.UnitTests.Validation
{
    [TestFixture]
    public class SyntaxValidationTests
    {
        [Test]
        public void TopLevelStatementsAreInvalid()
        {
            var parser = new ScriptParser();

            var parsed = parser.ParseScript(EmbeddedResources.GetScript("Validation.TopLevelStatement.txt"));

            var failure = Assert.Throws<CompilationDiagnosticFailureException>(() => parser.StrictCompile(parsed));

            Assert.That(failure.Failures.Single().Id, Is.EqualTo("CS0825"));
        }
    }
}