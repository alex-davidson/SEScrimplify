using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SEScrimplify.Analysis;

namespace SEScrimplify.Rewrites.Lambda
{
    class ScopeStructDefinition : IScopeContainerDefinition
    {
        public string Name { get; private set; }

        public ScopeStructDefinition(string name, IList<ISymbol> scopeCaptures)
        {
            Name = name;
            fields = scopeCaptures.ToDictionary(c => c, c => new CapturedSymbol(c));
            creationExpression = SyntaxFactory.ObjectCreationExpression(
                SyntaxFactory.ParseTypeName(name),
                SyntaxFactory.ArgumentList(),
                SyntaxFactory.InitializerExpression(
                    SyntaxKind.ObjectInitializerExpression,
                    SyntaxFactory.SeparatedList<ExpressionSyntax>(fields.Values.Select(f => f.FieldInitialisation)))).NormalizeWhitespace();
        }

        public IEnumerable<MemberDeclarationSyntax> GetTopLevelDeclarations()
        {
            yield return SyntaxFactory.StructDeclaration(Name)
                .WithMembers(
                    SyntaxFactory.List(
                        GetFieldDeclarations().Concat(GetMethodDeclarations())))
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)));
        }

        private IEnumerable<MemberDeclarationSyntax> GetFieldDeclarations()
        {
            return fields.Values.Select(f => f.FieldDeclaration);
        }

        private IEnumerable<MemberDeclarationSyntax> GetMethodDeclarations()
        {
            return lambdaMethods.Select(m => m.GetMethodDeclaration());
        }

        public ObjectCreationExpressionSyntax GetCreationExpression()
        {
            return creationExpression;
        }

        private readonly IDictionary<ISymbol, CapturedSymbol> fields;
        private readonly ObjectCreationExpressionSyntax creationExpression;
        private readonly List<ScopeMethodDefinition> lambdaMethods = new List<ScopeMethodDefinition>();

        public ILambdaMethodDefinition AddLambdaInstance(IGeneratedMemberNameProvider nameProvider, LambdaDefinition definition, BlockSyntax body)
        {
            var method = new ScopeMethodDefinition(nameProvider.NameLambdaMethod(definition), this, definition, body);
            lambdaMethods.Add(method);
            return method;
        }
        
        class CapturedSymbol
        {
            public ISymbol Symbol { get; private set; }

            public CapturedSymbol(ISymbol symbol)
            {
                Symbol = symbol;
                InternalReference = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.ThisExpression(), SyntaxFactory.IdentifierName(symbol.Name));
                FieldInitialisation = SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, SyntaxFactory.IdentifierName(symbol.Name), SyntaxFactory.IdentifierName(symbol.Name));

                var type = GetType(symbol);
                FieldDeclaration = SyntaxFactory.FieldDeclaration(
                    SyntaxFactory.VariableDeclaration(
                        SyntaxFactory.ParseTypeName(type.ToDisplayString()),
                        SyntaxFactory.SeparatedList(new[] { SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(symbol.Name)) })))
                    .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)));
            }

            private static ITypeSymbol GetType(ISymbol symbol)
            {
                if (symbol is IFieldSymbol)
                {
                    return ((IFieldSymbol)symbol).Type;
                }
                if (symbol is IPropertySymbol)
                {
                    return ((IPropertySymbol)symbol).Type;
                }
                if (symbol is ILocalSymbol)
                {
                    return ((ILocalSymbol)symbol).Type;
                }
                return null;
            }


            public AssignmentExpressionSyntax FieldInitialisation { get; private set; } // <field> = <arg>
            public MemberDeclarationSyntax FieldDeclaration { get; private set; }           // public <type> <field>
            public MemberAccessExpressionSyntax InternalReference { get; private set; }       // this.<field>
        }
    }
}