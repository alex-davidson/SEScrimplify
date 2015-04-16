using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SEScrimplify.Analysis;
using SEScrimplify.Rewrites.Lambda;

namespace SEScrimplify.Rewrites
{
    /// <summary>
    /// Walks the syntax tree identifying lambda functions, then rewrites the tree to
    /// use member functions of structs, etc instead.
    /// </summary>
    public class LambdaAsMemberFunctionRewrite : ISemanticallyAwareRewrite
    {
        private readonly IGeneratedMemberNameProvider nameProvider;

        public LambdaAsMemberFunctionRewrite(IGeneratedMemberNameProvider nameProvider)
        {
            this.nameProvider = nameProvider;
        }

        public SyntaxNode Rewrite(SyntaxNode root, SemanticModel semanticModel)
        {
            var collector = new LambdaDefinitionCollector(semanticModel);
            collector.Visit(root);

            var lambdas = collector.GetDefinitions().ToArray();
            if (!lambdas.Any()) return root;

            var structs = ResolveContainingScopes(lambdas);

            var rewrites = new RewriteList();
            foreach (var lambda in lambdas)
            {
                rewrites.Add(new RewriteAsMethodCall(nameProvider, lambda, structs[lambda]), lambda.SyntaxNode);
            }
            var rewritten = rewrites.ApplyRewrites(root);

            var scriptClass = rewritten.DescendantNodes().OfType<ClassDeclarationSyntax>().First();
            return rewritten.ReplaceNode(scriptClass, scriptClass.WithMembers(SyntaxFactory.List(scriptClass.Members.Concat(structs.Values.SelectMany(s => s.GetTopLevelDeclarations())))));
        }

        class RewriteAsMethodCall : ISyntaxNodeRewrite
        {
            private readonly IGeneratedMemberNameProvider nameProvider;
            private readonly LambdaDefinition lambda;
            private readonly IScopeContainerDefinition structDefinition;

            public RewriteAsMethodCall(IGeneratedMemberNameProvider nameProvider, LambdaDefinition lambda, IScopeContainerDefinition structDefinition)
            {
                this.nameProvider = nameProvider;
                this.lambda = lambda;
                this.structDefinition = structDefinition;
            }

            public SyntaxNode Rewrite(SyntaxNode original, SyntaxNode current)
            {
                return structDefinition.AddLambdaInstance(nameProvider, lambda, ConvertLambdaBodyToBlock(current)).GetMethodExpression();
            }

            private static BlockSyntax ConvertLambdaBodyToBlock(SyntaxNode lambdaSyntax)
            {
                if (lambdaSyntax is SimpleLambdaExpressionSyntax)
                {
                    return ((SimpleLambdaExpressionSyntax)lambdaSyntax).Body.ConvertLambdaBodyToBlock();
                }
                if(lambdaSyntax is ParenthesizedLambdaExpressionSyntax)
                {
                    return ((ParenthesizedLambdaExpressionSyntax)lambdaSyntax).Body.ConvertLambdaBodyToBlock();
                }
                throw new NotSupportedException(String.Format("Not a lambda? {0}", lambdaSyntax.GetType().Name));
            }
        }

        private Dictionary<LambdaDefinition, IScopeContainerDefinition> ResolveContainingScopes(LambdaDefinition[] definitions)
        {
            var structs = new Dictionary<LambdaDefinition, IScopeContainerDefinition>();
            foreach (var definition in definitions) ResolveContainingScope(structs, definition);
            return structs;
        }

        private void ResolveContainingScope(Dictionary<LambdaDefinition, IScopeContainerDefinition> structs, LambdaDefinition definition)
        {
            if (definition == null) return;
            if (structs.ContainsKey(definition)) return;

            ResolveContainingScope(structs, definition.ContainingLambda);

            if (!definition.AllReferences.Any())
            {
                structs.Add(definition, new TopLevelContainerDefinition());
            }
            else
            {
                structs.Add(definition, new ScopeStructDefinition(nameProvider.NameLambdaScopeStruct(), definition.AllReferences.ToList()));
            }
        }
    }
}