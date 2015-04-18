using System;
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
    public class LambdaAsMemberFunctionRewrite : IIndependentRewrite
    {
        private readonly IGeneratedMemberNameProvider nameProvider;

        public LambdaAsMemberFunctionRewrite(IGeneratedMemberNameProvider nameProvider)
        {
            this.nameProvider = nameProvider;
        }

        public SyntaxTree Rewrite(SyntaxTree tree, IScriptCompiler compiler)
        {
            var compilation = compiler.Compile(tree);
            var collector = new LambdaDefinitionCollector(compilation.GetSemanticModel(tree));

            var root = tree.GetRoot();
            collector.Visit(root);

            var lambdas = collector.GetModels().ToArray();
            if (!lambdas.Any()) return tree;

            var builder = new LambdaMethodsBuilder(nameProvider);
            var declarations = builder.ResolveScopesAndMethods(lambdas);

            var rewrites = new RewriteList();
            foreach (var declaration in declarations)
            {
                rewrites.Add(new RewriteAsMethodCall(declaration), declaration.Model.SyntaxNode);
                declaration.CollectSymbolRewrites(rewrites);
            }
            var rewritten = rewrites.ApplyRewrites(root);

            var scriptClass = rewritten.DescendantNodes().OfType<ClassDeclarationSyntax>().First();

            return SyntaxFactory.SyntaxTree(
                rewritten.ReplaceNode(scriptClass, scriptClass.WithMembers(
                    SyntaxFactory.List(scriptClass.Members.Concat(builder.GetTopLevelDeclarations())))));
        }

        class RewriteAsMethodCall : ISyntaxNodeRewrite
        {
            private readonly ILambdaMethodDeclaration declaration;

            public RewriteAsMethodCall(ILambdaMethodDeclaration declaration)
            {
                this.declaration = declaration;
            }

            public SyntaxNode Rewrite(SyntaxNode original, SyntaxNode current)
            {
                return declaration.DefineImplementation(ConvertLambdaBodyToBlock(current)).GetMethodCallExpression();
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
    }
}