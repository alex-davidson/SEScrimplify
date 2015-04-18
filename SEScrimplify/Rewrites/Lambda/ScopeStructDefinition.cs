using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SEScrimplify.Analysis;

namespace SEScrimplify.Rewrites.Lambda
{
    /// <summary>
    /// Defines a struct representing scope captured by a rewritten lambda.
    /// </summary>
    public class ScopeStructDefinition
    {
        public string Name { get; private set; }

        public ScopeStructDefinition(string name)
        {
            Name = name;
        }

        public ObjectCreationExpressionSyntax GetCreationExpression(FieldAssignments fieldAssignments, ISymbolMapping parentScope)
        {
            return SyntaxFactory.ObjectCreationExpression(
                SyntaxFactory.ParseTypeName(Name),
                SyntaxFactory.ArgumentList(),
                SyntaxFactory.InitializerExpression(
                    SyntaxKind.ObjectInitializerExpression,
                    SyntaxFactory.SeparatedList<ExpressionSyntax>(fieldAssignments.GetAssignmentExpressions(parentScope)))).NormalizeWhitespace();
        }

        public MemberDeclarationSyntax GetTopLevelDeclaration()
        {
            return SyntaxFactory.StructDeclaration(Name)
                .WithMembers(
                    SyntaxFactory.List(
                        GetFieldDeclarations().Concat(GetMethodDeclarations())))
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)));
        }

        public FieldAssignments AssignFields(IGeneratedMemberNameProvider nameProvider, IEnumerable<ISymbol> externalSymbols)
        {
            return new FieldAssignments(externalSymbols.ToDictionary(s => s, s => AssignField(nameProvider, s)));
        }

        private AvailableField AssignField(IGeneratedMemberNameProvider nameProvider, ISymbol symbol)
        {
            var field = new AvailableField(symbol.GetSymbolType(), nameProvider.NameLambdaScopeField(symbol));
            fields.Add(field);
            return field;
        }

        private IEnumerable<MemberDeclarationSyntax> GetFieldDeclarations()
        {
            return fields.Select(f => f.Declaration);
        }

        private IEnumerable<MemberDeclarationSyntax> GetMethodDeclarations()
        {
            return lambdaMethods.Select(m => m.GetMethodDeclaration());
        }

        private readonly List<AvailableField> fields = new List<AvailableField>();
        private readonly List<ScopeMethodDefinition> lambdaMethods = new List<ScopeMethodDefinition>();



        public ILambdaMethodDeclaration DeclareLambda(IGeneratedMemberNameProvider nameProvider, LambdaModel model, ISymbolMapping parentScope)
        {
            var fieldAssignments = AssignFields(nameProvider, model.AllReferences.Select(r => r.Symbol).Distinct().ToList());
            return new ScopeMethodDeclaration(nameProvider.NameLambdaMethod(model), this, model, fieldAssignments, parentScope);
        }

        public ILambdaMethodDefinition DefineLambda(ScopeMethodDeclaration declaration, BlockSyntax body)
        {
            var method = new ScopeMethodDefinition(declaration, body);
            lambdaMethods.Add(method);
            return method;
        }
    }
}