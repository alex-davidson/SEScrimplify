using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SEScrimplify.Rewrites
{
    public class ForeachAsWhileLoopRewrite : ISemanticallyAwareRewrite
    {
        private readonly IGeneratedMemberNameProvider nameProvider;

        public ForeachAsWhileLoopRewrite(IGeneratedMemberNameProvider nameProvider)
        {
            this.nameProvider = nameProvider;
        }

        public void CollectRewrites(RewriteList rewrites, SyntaxNode root, SemanticModel semanticModel)
        {
            var loops = root.DescendantNodes()
                .OfType<ForEachStatementSyntax>()
                .Select(l => new { SyntaxNode = l, Model = semanticModel.GetForEachStatementInfo(l), Iteratee = semanticModel.GetSymbolInfo(l.Expression) })
                .ToList();

            foreach (var loop in loops)
            {
                rewrites.Add(new RewriteAsWhileLoop(loop.Model, nameProvider.NameIterator(loop.Iteratee.Symbol)), loop.SyntaxNode);
            }
        }

        class RewriteAsWhileLoop : ISyntaxNodeRewrite
        {
            private readonly ForEachStatementInfo model;
            private SyntaxToken iteratorIdentifier;

            public RewriteAsWhileLoop(ForEachStatementInfo model, string iteratorName)
            {
                this.model = model;
                this.iteratorIdentifier = SyntaxFactory.Identifier(iteratorName);
            }

            public SyntaxNode Rewrite(SyntaxNode original, SyntaxNode current)
            {
                var node = (ForEachStatementSyntax)current;
                
                return SyntaxFactory.Block(SyntaxFactory.List(GetReplacementBlockStatements(node)));
            }

            private IEnumerable<StatementSyntax> GetReplacementBlockStatements(ForEachStatementSyntax node)
            {
                var getIterator = SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        node.Expression,
                        SyntaxFactory.IdentifierName(model.GetEnumeratorMethod.Name)));

                yield return SyntaxFactory.LocalDeclarationStatement(
                    SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName("var"),
                        SyntaxFactory.SeparatedList(new[] {
                            SyntaxFactory.VariableDeclarator(iteratorIdentifier).WithInitializer(SyntaxFactory.EqualsValueClause(getIterator))
                        })));

                yield return SyntaxFactory.WhileStatement(
                    SyntaxFactory.InvocationExpression(IteratorMember(model.MoveNextMethod.Name)),
                    node.Statement.PrependStatement(
                        SyntaxFactory.LocalDeclarationStatement(
                            SyntaxFactory.VariableDeclaration(node.Type,
                                SyntaxFactory.SeparatedList(new[] {
                                    SyntaxFactory.VariableDeclarator(node.Identifier).WithInitializer(SyntaxFactory.EqualsValueClause(GetConvertedCurrentValue()))
                                })))));

                if (EnumeratorNeedsDisposal())
                {
                    yield return SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.InvocationExpression(
                            IteratorMember(model.DisposeMethod.Name)));
                }
            }
            private bool EnumeratorNeedsDisposal()
            {
                if(model.DisposeMethod == null) return false;
                if(model.GetEnumeratorMethod.ReturnType.FindImplementationForInterfaceMember(model.DisposeMethod) == null) return false;

                return true;
            }

            private ExpressionSyntax GetConvertedCurrentValue()
            {
                var currentValue = IteratorMember(model.CurrentProperty.Name);
                var conversion = model.CurrentConversion;
                if (conversion.IsIdentity) return currentValue;

                if(conversion.IsExplicit)
                {
                    return SyntaxFactory.CastExpression(model.ElementType.AsSyntax(), currentValue);
                }
                throw new NotSupportedException();
            }

            private MemberAccessExpressionSyntax IteratorMember(string memberName)
            {
                return SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName(iteratorIdentifier),
                    SyntaxFactory.IdentifierName(memberName));
            }
        }
    }
}
