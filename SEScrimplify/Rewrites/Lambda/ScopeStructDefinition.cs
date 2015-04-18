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

        public ObjectCreationExpressionSyntax GetCreationExpression(IDictionary<ISymbol, AvailableField> symbolMappings)
        {
            return SyntaxFactory.ObjectCreationExpression(
                SyntaxFactory.ParseTypeName(Name),
                SyntaxFactory.ArgumentList(),
                SyntaxFactory.InitializerExpression(
                    SyntaxKind.ObjectInitializerExpression,
                    SyntaxFactory.SeparatedList<ExpressionSyntax>(symbolMappings.Select(m => CreateFieldInitialisation(m.Key, m.Value))))).NormalizeWhitespace();
        }

        private static AssignmentExpressionSyntax CreateFieldInitialisation(ISymbol symbol, AvailableField field)
        {
            return SyntaxFactory.AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    SyntaxFactory.IdentifierName(field.Name),
                    SyntaxFactory.IdentifierName(symbol.Name));
        }

        public MemberDeclarationSyntax GetTopLevelDeclaration()
        {
            return SyntaxFactory.StructDeclaration(Name)
                .WithMembers(
                    SyntaxFactory.List(
                        GetFieldDeclarations().Concat(GetMethodDeclarations())))
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)));
        }

        public IDictionary<ISymbol, AvailableField> AssignFields(IGeneratedMemberNameProvider nameProvider, IEnumerable<ISymbol> externalSymbols)
        {
            return externalSymbols.ToDictionary(s => s, s => AssignField(nameProvider, s));
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

        public ILambdaMethodDefinition AddLambdaInstance(IGeneratedMemberNameProvider nameProvider, LambdaDefinition definition, BlockSyntax body, IDictionary<ISymbol, AvailableField> symbolMappings)
        {
            var method = new ScopeMethodDefinition(nameProvider.NameLambdaMethod(definition), this, definition, body, symbolMappings);
            lambdaMethods.Add(method);
            return method;
        }
    }
}