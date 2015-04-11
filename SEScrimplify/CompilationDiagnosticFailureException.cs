using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace SEScrimplify
{
    public class CompilationDiagnosticFailureException : Exception
    {
        public CSharpCompilation Compilation { get; private set; }
        public Diagnostic[] Failures { get; private set; }

        public CompilationDiagnosticFailureException(CSharpCompilation compilation, Diagnostic[] failures)
            : base(String.Join(Environment.NewLine, failures.Select(f => f.GetMessage()).ToArray()))
        {
            Compilation = compilation;
            Failures = failures;
        }
    }
}