using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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

        public Dictionary<LambdaDefinition, ILambdaDeclaration> ResolveContainingScopes(LambdaDefinition[] definitions)
        {
            var instances = new Dictionary<LambdaDefinition, ILambdaDeclaration>();
            foreach (var definition in definitions) ResolveContainingScope(instances, definition);
            return instances;
        }

        private void ResolveContainingScope(Dictionary<LambdaDefinition, ILambdaDeclaration> instances, LambdaDefinition definition)
        {
            if (definition == null) return;
            if (instances.ContainsKey(definition)) return;

            ResolveContainingScope(instances, definition.ContainingLambda);

            if (!definition.AllReferences.Any())
            {
                instances.Add(definition, new TopLevelMethodDeclaration(this, definition));
            }
            else
            {
                var structDef = new ScopeStructDefinition(nameProvider.NameLambdaScopeStruct());
                structs.Add(structDef);
                instances.Add(definition, structDef.DeclareLambda(nameProvider, definition));
            }
        }
        
        private ILambdaMethodDefinition AddTopLevelLambdaInstance(LambdaDefinition definition, BlockSyntax body)
        {
            var method = new TopLevelMethodDefinition("Lambda_" + nameProvider.NameLambdaMethod(definition), definition, body);
            staticMethods.Add(method);
            return method;
        }

        class TopLevelMethodDeclaration : ILambdaDeclaration
        {
            private readonly LambdaMethodsBuilder builder;
            private readonly LambdaDefinition definition;

            public TopLevelMethodDeclaration(LambdaMethodsBuilder builder, LambdaDefinition definition)
            {
                this.builder = builder;
                this.definition = definition;
            }

            public ILambdaMethodDefinition DefineLambda(BlockSyntax body)
            {
                return builder.AddTopLevelLambdaInstance(definition, body);
            }
        }
    }
}