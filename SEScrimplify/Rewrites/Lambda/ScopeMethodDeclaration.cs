﻿using System.Linq;
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
        private readonly ISymbolMapping parentScope;

        public ScopeMethodDeclaration(string name, ScopeStructDefinition owner, LambdaDefinition definition, FieldAssignments fieldAssignments, ISymbolMapping parentScope)
        {
            this.owner = owner;
            this.Definition = definition;
            this.fieldAssignments = fieldAssignments;
            this.parentScope = parentScope;
            MethodName = SyntaxFactory.IdentifierName(name);
        }

        public IdentifierNameSyntax MethodName { get; private set; }

        public ExpressionSyntax GetMethodCallExpression()
        {
            return SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, owner.GetCreationExpression(fieldAssignments, parentScope), MethodName);
        }

        public ILambdaMethodDefinition DefineLambda(BlockSyntax body)
        {
            return owner.DefineLambda(this, body);
        }

        public void AddSymbolRewrites(RewriteList rewrites)
        {
            foreach(var symbolRefs in Definition.DirectReferences
                .Where(r => fieldAssignments.HasField(r.Symbol))
                .GroupBy(r => r.Symbol, r => r.SyntaxNode))
            {
                var field = fieldAssignments.GetAssignedField(symbolRefs.Key);
                var rewrite = new RewriteAsFieldAccess(field);
                rewrites.Add(rewrite, symbolRefs.ToArray());
            }
        }

        class RewriteAsFieldAccess : ISyntaxNodeRewrite
        {
            private readonly MemberAccessExpressionSyntax node;

            public RewriteAsFieldAccess(AvailableField field)
            {
                this.node = field.CreateReference(SyntaxFactory.ThisExpression()); ;
            }

            public SyntaxNode Rewrite(SyntaxNode original, SyntaxNode current)
            {
                return node;
            }
        }


        public ISymbolMapping SymbolMapping
        {
            get { return fieldAssignments; }
        }
    }
}