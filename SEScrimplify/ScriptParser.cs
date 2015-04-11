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
        /// The resulting tree will contain some additional boilerplate in order to make it compile as a C# library.
        /// </remarks>
        /// <param name="script"></param>
        /// <returns></returns>
        public SyntaxTree ParseScript(string script)
        {
            var builder = new ScriptBuilder();
            builder.AddScript(script);
            return builder.ToSyntaxTree();
        }

        public CSharpCompilation Compile(SyntaxTree tree)
        {
            return CSharpCompilation.Create("Internal").AddSyntaxTrees(tree)
                .AddReferences(MetadataReference.CreateFromAssembly(typeof(Object).Assembly)) // Add reference mscorlib.dll
                .AddReferences(MetadataReference.CreateFromAssembly(typeof(Enumerable).Assembly)) // Add reference System.Core.dll;
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, scriptClassName: "__Script__"));
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
