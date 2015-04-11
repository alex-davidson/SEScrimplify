using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SEScrimplify
{
    public class ScriptParser
    {
        /// <summary>
        /// Parses a Space Engineers script into a valid C# syntax tree.
        /// </summary>
        /// <remarks>
        /// The resulting tree will contain some additional boilerplate in order to make it compile as a C# program.
        /// </remarks>
        /// <param name="script"></param>
        /// <returns></returns>
        public SyntaxTree ParseScript(string script)
        {
            var node = SyntaxFactory.ParseCompilationUnit(script);

            var syntaxDiagnostics = node.GetDiagnostics().Where(d => d.WarningLevel <= 0).ToArray();
            if (syntaxDiagnostics.Any()) throw new SyntaxDiagnosticFailureException(node.SyntaxTree, syntaxDiagnostics.ToArray());

            return CreateValidTree(node);
        }

        /// <summary>
        /// Wraps bare script members in a __Script class containing a static Main() method.
        /// Permits compilation as a C# program.
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        private static SyntaxTree CreateValidTree(CompilationUnitSyntax unit)
        {
            var fakeEntryPoint = SyntaxFactory.ParseCompilationUnit("static void Main(string[] args) { new __Script__().Main(); }");

            var mainMethod = unit.Members.OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.Text == "Main").ToList();
            if (!mainMethod.Any()) throw new SyntaxDiagnosticFailureException(unit.SyntaxTree, "No Main() method has been defined.");
            if (!mainMethod.Any(m => m.Arity == 0)) throw new SyntaxDiagnosticFailureException(unit.SyntaxTree, "Main() method must not take any arguments.");

            return SyntaxFactory.SyntaxTree(
                SyntaxFactory.CompilationUnit(
                    unit.Externs,
                    unit.Usings,
                    SyntaxFactory.List<AttributeListSyntax>(),
                    SyntaxFactory.List<MemberDeclarationSyntax>(
                        new[] {
                            SyntaxFactory.ClassDeclaration("__Script__").WithMembers(
                                SyntaxFactory.List(fakeEntryPoint.Members.Concat(unit.Members))).NormalizeWhitespace()
                        })));

        }

        public CSharpCompilation Compile(SyntaxTree tree)
        {
            return CSharpCompilation.Create("Internal").AddSyntaxTrees(tree)
                .AddReferences(MetadataReference.CreateFromAssembly(typeof(Object).Assembly)) // Add reference mscorlib.dll
                .AddReferences(MetadataReference.CreateFromAssembly(typeof(Enumerable).Assembly)); // Add reference System.Core.dll;
        }

        public CSharpCompilation StrictCompile(SyntaxTree tree)
        {
            var compilation = Compile(tree);
            var compilationDiagnostics = compilation.GetDiagnostics().Where(d => d.WarningLevel <= 0).ToArray();
            if (compilationDiagnostics.Any()) throw new CompilationDiagnosticFailureException(compilation, compilationDiagnostics.ToArray());

            return compilation;
        }

        public SemanticModel GetSemanticModel(CSharpCompilation compilation, SyntaxTree tree)
        {
            return compilation.GetSemanticModel(tree);
        }
    }

}
