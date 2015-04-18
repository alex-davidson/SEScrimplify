using Microsoft.CodeAnalysis;
using SEScrimplify.Analysis;

namespace SEScrimplify.Rewrites
{
    public interface IGeneratedMemberNameProvider
    {
        string NameLambdaScopeStruct();
        string NameLambdaScopeField(ISymbol symbol);
        string NameLambdaMethod(LambdaModel model);
    }
}