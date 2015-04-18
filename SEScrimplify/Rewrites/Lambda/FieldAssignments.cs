using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SEScrimplify.Rewrites.Lambda
{
    public class FieldAssignments : IEnumerable<KeyValuePair<ISymbol, AvailableField>>
    {
        private readonly IDictionary<ISymbol, AvailableField> assignments;

        public FieldAssignments(IDictionary<ISymbol, AvailableField> assignments)
        {
            this.assignments = assignments;
        }

        public AvailableField GetAssignedField(ISymbol symbol)
        {
            return assignments[symbol];
        }

        public IEnumerable<AssignmentExpressionSyntax> GetAssignmentExpressions()
        {
            return assignments.Select(a => CreateAssignment(a.Key, a.Value));
        }

        private static AssignmentExpressionSyntax CreateAssignment(ISymbol symbol, AvailableField field)
        {
            return SyntaxFactory.AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                SyntaxFactory.IdentifierName(field.Name),
                SyntaxFactory.IdentifierName(symbol.Name));
        }

        public IEnumerator<KeyValuePair<ISymbol, AvailableField>> GetEnumerator()
        {
            return assignments.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}