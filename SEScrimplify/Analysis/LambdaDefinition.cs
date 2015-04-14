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

        public LambdaDefinition(SyntaxNode syntaxNode, CSharpSyntaxNode methodBody, TypeSyntax returnType, LambdaDefinition containingLambda, IParameterSymbol[] parameters)
        {
            SyntaxNode = syntaxNode;
            ReturnType = returnType;
            ContainingLambda = containingLambda;
            Parameters = parameters;
            DirectReferences = new HashSet<ISymbol>();
            AllReferences = new HashSet<ISymbol>();

            methodString = methodBody.DescendantNodes().OfType<MemberAccessExpressionSyntax>().Select(n => n.Name.Identifier.Text).FirstOrDefault() ?? "func";
        }

        public void AddDirectReference(ISymbol symbol)
        {
            DirectReferences.Add(symbol);
            AllReferences.Add(symbol);
            var parent = ContainingLambda;
            while (parent != null)
            {
                parent.AllReferences.Add(symbol);
                parent = ContainingLambda;
            }
        }

        public ICollection<ISymbol> AllReferences { get; private set; }

        public IParameterSymbol[] Parameters { get; private set; }
        public ICollection<ISymbol> DirectReferences { get; private set; }
        public LambdaDefinition ContainingLambda { get; private set; }

        public SyntaxNode SyntaxNode { get; private set; }
        public TypeSyntax ReturnType { get; private set; }

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
    }
}