using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SEScrimplify.Analysis;

namespace SEScrimplify.Rewrites.Lambda
{
    class ScopeStructDefinition
    {
        public string Name { get; private set; }

        public ScopeStructDefinition(string name)
        {
            Name = name;
        }

        public ObjectCreationExpressionSyntax GetCreationExpression(FieldAssignments fieldAssignments)
        {
            return SyntaxFactory.ObjectCreationExpression(
                SyntaxFactory.ParseTypeName(Name),
                SyntaxFactory.ArgumentList(),
                SyntaxFactory.InitializerExpression(
                    SyntaxKind.ObjectInitializerExpression,
                    SyntaxFactory.SeparatedList<ExpressionSyntax>(fieldAssignments.GetAssignmentExpressions()))).NormalizeWhitespace();
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



        public ILambdaMethodDefinition AddLambdaInstance(IGeneratedMemberNameProvider nameProvider, LambdaDefinition definition, BlockSyntax body, FieldAssignments fieldAssignments)
        {
            var method = new ScopeMethodDefinition(nameProvider.NameLambdaMethod(definition), this, definition, body, fieldAssignments);
            lambdaMethods.Add(method);
            return method;
        }
    }
}