using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SEScrimplify.Analysis;

namespace SEScrimplify.Rewrites.Lambda
{
    public class LambdaMethodsBuilder
    {
        private readonly IGeneratedMemberNameProvider nameProvider;

        public LambdaMethodsBuilder(IGeneratedMemberNameProvider nameProvider)
        {
            this.nameProvider = nameProvider;
        }

        private readonly List<TopLevelMethodDefinition> staticMethods = new List<TopLevelMethodDefinition>();
        private readonly List<ScopeStructDefinition> structs = new List<ScopeStructDefinition>();


        public IEnumerable<MemberDeclarationSyntax> GetTopLevelDeclarations()
        {
            return structs.Select(s => s.GetTopLevelDeclaration())
                .Concat(staticMethods.Select(s => s.GetTopLevelDeclaration()));
        }

        public IEnumerable<ILambdaMethodDeclaration> ResolveScopesAndMethods(LambdaModel[] models)
        {
            var instances = new Dictionary<LambdaModel, ILambdaMethodDeclaration>();
            foreach (var model in models) ResolveContainingScope(instances, model);
            return instances.Values;
        }

        private ILambdaMethodDeclaration ResolveContainingScope(Dictionary<LambdaModel, ILambdaMethodDeclaration> instances, LambdaModel model)
        {
            if (model == null) return null;
            if (instances.ContainsKey(model)) return instances[model];

            var containingLambda = ResolveContainingScope(instances, model.ContainingLambda);

            var declaration = CreateLambdaDeclaration(model, containingLambda);
            instances.Add(model, declaration);
            return declaration;
        }

        private ILambdaMethodDeclaration CreateLambdaDeclaration(LambdaModel model, ILambdaMethodDeclaration containingLambda)
        {
            if (!model.AllReferences.Any()) return new TopLevelMethodDeclaration(this, model);

            var structDef = new ScopeStructDefinition(nameProvider.NameLambdaScopeStruct());
            structs.Add(structDef);
            var symbolMapping = containingLambda == null ? new NoSymbolMapping() : containingLambda.SymbolMapping;
            return structDef.DeclareLambda(nameProvider, model, symbolMapping);
        }

        private ILambdaMethodDefinition AddTopLevelLambdaInstance(LambdaModel model, BlockSyntax body)
        {
            var method = new TopLevelMethodDefinition("Lambda_" + nameProvider.NameLambdaMethod(model), model, body);
            staticMethods.Add(method);
            return method;
        }

        class TopLevelMethodDeclaration : ILambdaMethodDeclaration
        {
            private readonly LambdaMethodsBuilder builder;
            public LambdaModel Model { get; private set; }

            public TopLevelMethodDeclaration(LambdaMethodsBuilder builder, LambdaModel model)
            {
                this.builder = builder;
                Model = model;
            }

            public ILambdaMethodDefinition DefineImplementation(BlockSyntax body)
            {
                return builder.AddTopLevelLambdaInstance(Model, body);
            }

            public void CollectSymbolRewrites(RewriteList rewrites)
            {
            }

            public ISymbolMapping SymbolMapping
            {
                get { return new NoSymbolMapping(); }
            }
        }
    }
}