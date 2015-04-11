using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SEScrimplify
{
    /// <summary>
    /// Combines multiple scripts into a single valid SyntaxTree which can be compiled as a C# program.
    /// </summary>
    public class ScriptBuilder
    {
        readonly List<CompilationUnitSyntax> units = new List<CompilationUnitSyntax>();

        private CompilationUnitSyntax ParseScriptUnit(string script)
        {
            var node = SyntaxFactory.ParseCompilationUnit(script);
            var syntaxDiagnostics = units.SelectMany(u => u.GetDiagnostics()).Where(d => d.WarningLevel <= 0).ToArray();
            if (syntaxDiagnostics.Any()) throw new SyntaxDiagnosticFailureException(node.SyntaxTree, syntaxDiagnostics.ToArray());
            return node;
        }

        public void AddScript(string content)
        {
            units.Add(ParseScriptUnit(content));
        }

        public SyntaxTree ToSyntaxTree()
        {
            // Wrap bare script members in a __Script__ class. Permits compilation as a C# library.
            var tree = SyntaxFactory.SyntaxTree(
                SyntaxFactory.CompilationUnit(
                    SyntaxFactory.List(units.SelectMany(u => u.Externs).Distinct()),
                    SyntaxFactory.List(units.SelectMany(u => u.Usings).Distinct()),
                    SyntaxFactory.List<AttributeListSyntax>(),
                    SyntaxFactory.List<MemberDeclarationSyntax>(
                        new[] {
                            SyntaxFactory.ClassDeclaration("__Script__").WithMembers(
                                SyntaxFactory.List(units.SelectMany(u => u.Members))).NormalizeWhitespace()
                        })));

            var mainMethod = units.SelectMany(u => u.Members).OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.Text == "Main").ToList();
            if (!mainMethod.Any()) throw new SyntaxDiagnosticFailureException(tree, "No Main() method has been defined.");
            if (!mainMethod.Any(m => m.Arity == 0)) throw new SyntaxDiagnosticFailureException(tree, "Main() method must not take any arguments.");

            return tree;
        }
    }
}