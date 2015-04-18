using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SEScrimplify.Analysis;

namespace SEScrimplify.Rewrites.Lambda
{
    /// <summary>
    /// Declaration of a scope-bound method to replace a lambda.
    /// </summary>
    /// <remarks>
    /// Contains enough information to:
    ///  * Write a call expression for this method,
    ///  * Rewrite external symbol references.
    /// Does not contain concrete information about the method's implementation.
    /// </remarks>
    class ScopeMethodDeclaration : ILambdaDeclaration
    {
        private readonly ScopeStructDefinition owner;
        public LambdaDefinition Definition { get; private set; }
        private readonly FieldAssignments fieldAssignments;

        public ScopeMethodDeclaration(string name, ScopeStructDefinition owner, LambdaDefinition definition, FieldAssignments fieldAssignments)
        {
            this.owner = owner;
            this.Definition = definition;
            this.fieldAssignments = fieldAssignments;
            MethodName = SyntaxFactory.IdentifierName(name);
        }

        public IdentifierNameSyntax MethodName { get; private set; }

        public ExpressionSyntax GetMethodCallExpression()
        {
            return SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, owner.GetCreationExpression(fieldAssignments), MethodName);
        }

        public ILambdaMethodDefinition DefineLambda(BlockSyntax body)
        {
            return owner.DefineLambda(this, body);
        }
    }
}