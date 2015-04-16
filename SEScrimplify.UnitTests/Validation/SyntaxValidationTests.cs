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

            var parsed = parser.ParseScript("new List<string>(); void Main() { }");
            Assert.Throws<CompilationDiagnosticFailureException>(() => parser.StrictCompile(parsed));
        }

        [Test]
        public void AbsentMainMethodIsInvalid()
        {
            var parser = new ScriptParser();

            var parsed = parser.ParseScript("void NotAMainMethod() {}");

            Assert.Throws<SyntaxDiagnosticFailureException>(() => parser.StrictCompile(parsed), "No Main() method");
        }

        [Test]
        public void SingleMainMethodWithArgumentsIsInvalid()
        {
            var parser = new ScriptParser();

            var parsed = parser.ParseScript("void Main(int argument) {}");

            Assert.Throws<SyntaxDiagnosticFailureException>(() => parser.StrictCompile(parsed), "must not take any arguments");
        }
        [Test]
        public void SingleNonVoidMainMethodIsInvalid()
        {
            var parser = new ScriptParser();

            var parsed = parser.ParseScript("int Main() {}");

            Assert.Throws<SyntaxDiagnosticFailureException>(() => parser.StrictCompile(parsed), "must return void");
        }
    }
}