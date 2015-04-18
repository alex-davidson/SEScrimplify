using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SEScrimplify.Analysis
{
    public class LambdaModel
    {
        private readonly string methodString;

        public LambdaModel(SyntaxNode syntaxNode, CSharpSyntaxNode methodBody, ITypeSymbol returnType, LambdaModel containingLambda, IParameterSymbol[] parameters)
        {
            SyntaxNode = syntaxNode;
            ReturnType = returnType;
            ContainingLambda = containingLambda;
            Parameters = parameters;
            DirectReferences = new List<SymbolReference>();
            Declarations = new HashSet<ISymbol>();
            AllReferences = new List<SymbolReference>();

            methodString = methodBody.GetResultExpressions()
                .SelectMany(r => r.DescendantNodes().OfType<MemberAccessExpressionSyntax>())
                .Select(n => n.Name.Identifier.Text).FirstOrDefault() ?? "func";
        }

        public void AddDirectReference(ISymbol symbol, SyntaxNode syntaxNode)
        {
            if (Declarations.Contains(symbol)) return;
            var reference = new SymbolReference(symbol, syntaxNode);
            DirectReferences.Add(reference);
            AddReference(reference);
        }

        public void AddDeclaration(ISymbol symbol)
        {
            Declarations.Add(symbol);
        }

        /// <summary>
        /// References to external symbols made from anywhere inside this lambda, including child lambdas.
        /// </summary>
        public ICollection<SymbolReference> AllReferences { get; private set; }

        public IParameterSymbol[] Parameters { get; private set; }

        /// <summary>
        /// References to external symbols made directly from this lambda.
        /// </summary>
        public ICollection<SymbolReference> DirectReferences { get; private set; }
        public ICollection<ISymbol> Declarations { get; private set; }
        public LambdaModel ContainingLambda { get; private set; }

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

        private void AddReference(SymbolReference reference)
        {
            if (Declarations.Contains(reference.Symbol)) return;
            if (Parameters.Contains(reference.Symbol)) return;
            AllReferences.Add(reference);
            if (ContainingLambda == null) return;
            ContainingLambda.AddReference(reference);
        }
    }
}