using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SEScrimplify.Rewrites.Lambda
{
    public class FieldAssignments : IEnumerable<KeyValuePair<ISymbol, AvailableField>>, ISymbolMapping
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

        public bool HasField(ISymbol symbol)
        {
            return assignments.ContainsKey(symbol);
        }

        public IEnumerable<AssignmentExpressionSyntax> GetAssignmentExpressions(ISymbolMapping symbolMapping)
        {
            return assignments.Select(a => CreateAssignment(a.Key, a.Value, symbolMapping));
        }

        private static AssignmentExpressionSyntax CreateAssignment(ISymbol symbol, AvailableField field, ISymbolMapping symbolMapping)
        {
            ExpressionSyntax symbolAccessExpression = symbolMapping.GetMappedMemberAccessExpression(symbol);

            return SyntaxFactory.AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                SyntaxFactory.IdentifierName(field.Name),
                symbolAccessExpression ?? SyntaxFactory.IdentifierName(symbol.Name));
        }

        public IEnumerator<KeyValuePair<ISymbol, AvailableField>> GetEnumerator()
        {
            return assignments.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        MemberAccessExpressionSyntax ISymbolMapping.GetMappedMemberAccessExpression(ISymbol symbol)
        {
            AvailableField field;
            if (!assignments.TryGetValue(symbol, out field)) return null;// No mapping required.
            return field.CreateReference(SyntaxFactory.ThisExpression());
        }
    }
}