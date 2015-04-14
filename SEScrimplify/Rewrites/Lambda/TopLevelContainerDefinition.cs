using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SEScrimplify.Analysis;

namespace SEScrimplify.Rewrites.Lambda
{
    class TopLevelContainerDefinition : IScopeContainerDefinition
    {
        private readonly List<TopLevelMethodDefinition> methods = new List<TopLevelMethodDefinition>();

        public ILambdaMethodDefinition AddLambdaInstance(IGeneratedMemberNameProvider nameProvider, LambdaDefinition definition, BlockSyntax body)
        {
            var method = new TopLevelMethodDefinition("Lambda_" + nameProvider.NameLambdaMethod(definition), definition, body);
            methods.Add(method);
            return method;
        }
        public IEnumerable<MemberDeclarationSyntax> GetTopLevelDeclarations()
        {
            return methods.Select(m => m.GetMethodDeclaration());
        }
    }
}