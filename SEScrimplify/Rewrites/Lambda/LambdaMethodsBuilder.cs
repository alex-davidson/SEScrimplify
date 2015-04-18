using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SEScrimplify.Analysis;

namespace SEScrimplify.Rewrites.Lambda
{
    public class LambdaMethodsBuilder : ILambdaDeclaration
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
                instances.Add(definition, this);
            }
            else
            {
                var structDef = new ScopeStructDefinition(nameProvider.NameLambdaScopeStruct());
                structs.Add(structDef);
                instances.Add(definition, new StructScopeLambdaMethodDeclaration(this, structDef));
            }
        }
        
        private ILambdaMethodDefinition AddTopLevelLambdaInstance(LambdaDefinition definition, BlockSyntax body)
        {
            var method = new TopLevelMethodDefinition("Lambda_" + nameProvider.NameLambdaMethod(definition), definition, body);
            staticMethods.Add(method);
            return method;
        }

        private ILambdaMethodDefinition AddStructScopeLambdaInstance(ScopeStructDefinition structDef, LambdaDefinition definition, BlockSyntax body)
        {
            var fieldAssignments = structDef.AssignFields(nameProvider, definition.AllReferences.Select(r => r.Symbol).Distinct().ToList());
            return structDef.AddLambdaInstance(nameProvider, definition, body, fieldAssignments);
        }

        ILambdaMethodDefinition ILambdaDeclaration.DefineLambda(LambdaDefinition definition, BlockSyntax body)
        {
            return AddTopLevelLambdaInstance(definition, body);
        }

        private class StructScopeLambdaMethodDeclaration : ILambdaDeclaration
        {
            private readonly LambdaMethodsBuilder builder;
            private readonly ScopeStructDefinition structDef;

            public StructScopeLambdaMethodDeclaration(LambdaMethodsBuilder builder, ScopeStructDefinition structDef)
            {
                this.builder = builder;
                this.structDef = structDef;
            }

            public ILambdaMethodDefinition DefineLambda(LambdaDefinition definition, BlockSyntax body)
            {
                return builder.AddStructScopeLambdaInstance(structDef, definition, body);
            }
        }
    }
}