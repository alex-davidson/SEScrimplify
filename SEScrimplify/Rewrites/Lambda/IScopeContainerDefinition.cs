using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SEScrimplify.Analysis;

namespace SEScrimplify.Rewrites.Lambda
{
    public interface IScopeContainerDefinition
    {
        ILambdaMethodDefinition AddLambdaInstance(IGeneratedMemberNameProvider nameProvider, LambdaDefinition definition, BlockSyntax body);
        IEnumerable<MemberDeclarationSyntax> GetTopLevelDeclarations();
    }
}