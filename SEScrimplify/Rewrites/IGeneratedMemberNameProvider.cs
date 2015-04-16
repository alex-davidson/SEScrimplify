using SEScrimplify.Analysis;

namespace SEScrimplify.Rewrites
{
    public interface IGeneratedMemberNameProvider
    {
        string NameLambdaScopeStruct();
        string NameLambdaMethod(LambdaDefinition definition);
    }
}