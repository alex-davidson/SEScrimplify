using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace SEScrimplify
{
    public class SyntaxDiagnosticFailureException : Exception
    {
        public SyntaxTree Syntax { get; private set; }
        public Diagnostic[] Failures { get; private set; }

        public SyntaxDiagnosticFailureException(SyntaxTree syntax, Diagnostic[] failures)
            : base(String.Join(Environment.NewLine, failures.Select(f => f.ToString()).ToArray()))
        {
            Syntax = syntax;
            Failures = failures;
        }

        public SyntaxDiagnosticFailureException(SyntaxTree syntax, string failureMessage)
            : base(failureMessage)
        {
            Syntax = syntax;
            Failures = new Diagnostic[0];
        }
    }
}