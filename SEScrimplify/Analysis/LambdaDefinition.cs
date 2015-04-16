using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SEScrimplify.Analysis
{
    public class LambdaDefinition
    {
        private readonly string methodString;

        public LambdaDefinition(SyntaxNode syntaxNode, CSharpSyntaxNode methodBody, ITypeSymbol returnType, LambdaDefinition containingLambda, IParameterSymbol[] parameters)
        {
            SyntaxNode = syntaxNode;
            ReturnType = returnType;
            ContainingLambda = containingLambda;
            Parameters = parameters;
            DirectReferences = new HashSet<ISymbol>();
            Declarations = new HashSet<ISymbol>();
            AllReferences = new HashSet<ISymbol>();

            methodString = methodBody.GetResultExpressions()
                .SelectMany(r => r.DescendantNodes().OfType<MemberAccessExpressionSyntax>())
                .Select(n => n.Name.Identifier.Text).FirstOrDefault() ?? "func";
        }

        public void AddDirectReference(ISymbol symbol)
        {
            if (Declarations.Contains(symbol)) return;
            DirectReferences.Add(symbol);
            AddReference(symbol);
        }

        public void AddDeclaration(ISymbol symbol)
        {
            Declarations.Add(symbol);
        }
        public ICollection<ISymbol> AllReferences { get; private set; }

        public IParameterSymbol[] Parameters { get; private set; }
        public ICollection<ISymbol> DirectReferences { get; private set; }
        public ICollection<ISymbol> Declarations { get; private set; }
        public LambdaDefinition ContainingLambda { get; private set; }

        public SyntaxNode SyntaxNode { get; private set; }
        public ITypeSymbol ReturnType { get; private set; }

        public TypeSyntax GetReturnTypeSyntax()
        {
            return SyntaxFactory.ParseTypeName(ReturnType.ToDisplayString());
        }

        public ParameterListSyntax GetParameterListSyntax()
        {
            return SyntaxFactory.ParameterList(
                SyntaxFactory.SeparatedList(
                    Parameters.Select(p =>
                        SyntaxFactory.Parameter(SyntaxFactory.Identifier(p.Name))
                            .WithType(SyntaxFactory.ParseTypeName(p.Type.ToDisplayString()))))); 
        }

        /// <summary>
        /// Get some sort of 'representative' string from the method body. Used for naming the method.
        /// </summary>
        /// <returns></returns>
        public string GetRelatedMethodString()
        {
            return methodString;
        }

        private void AddReference(ISymbol symbol)
        {
            if (Declarations.Contains(symbol)) return;
            AllReferences.Add(symbol);
            if (ContainingLambda == null) return;
            ContainingLambda.AddReference(symbol);
        }
    }
}