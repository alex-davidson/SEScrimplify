using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SEScrimplify.Analysis;

namespace SEScrimplify.Rewrites
{
    /// <summary>
    /// Walks the syntax tree identifying lambda functions, then rewrites the tree to
    /// use member functions of structs, etc instead.
    /// </summary>
    public class LambdaAsMemberFunctionRewrite : ISemanticallyAwareRewrite
    {
        public SyntaxNode Rewrite(SyntaxNode root, SemanticModel semanticModel)
        {
            var collector = new LambdaDefinitionCollector(semanticModel);
            collector.Visit(root);

            var lambdas = collector.GetDefinitions().ToArray();
            if (!lambdas.Any()) return root;

            var structs = GenerateStructHierarchy(lambdas);

            var rewrites = new RewriteList();
            foreach(var lambda in lambdas)
            {
                rewrites.Add(new RewriteAsMethodCall(lambda, structs[lambda]), lambda.SyntaxNode);
            }
            var scriptClass = root.DescendantNodes().OfType<ClassDeclarationSyntax>().First();
            rewrites.Add(new AddStructDefinitionsToClass(structs.Values), scriptClass);

            return rewrites.ApplyRewrites(root);
        }

        class RewriteAsMethodCall : ISyntaxNodeRewrite
        {
            private readonly LambdaDefinition lambda;
            private readonly SyntaxNode structDefinition;

            public RewriteAsMethodCall(LambdaDefinition lambda, SyntaxNode structDefinition)
            {
                this.lambda = lambda;
                this.structDefinition = structDefinition;
            }

            public SyntaxNode Rewrite(SyntaxNode original, SyntaxNode current)
            {
                return current;
            }
        }

        class AddStructDefinitionsToClass : ISyntaxNodeRewrite
        {
            private readonly List<SyntaxNode> structs;

            public AddStructDefinitionsToClass(ICollection<SyntaxNode> structs)
            {
                this.structs = structs.Where(s => s.Parent == null).ToList();
            }

            public SyntaxNode Rewrite(SyntaxNode original, SyntaxNode current)
            {
                var classDeclaration = (ClassDeclarationSyntax)current;

                return classDeclaration;
            }
        }

        private Dictionary<LambdaDefinition, SyntaxNode> GenerateStructHierarchy(LambdaDefinition[] definitions)
        {
            return new Dictionary<LambdaDefinition,SyntaxNode>();
        }

    }
}