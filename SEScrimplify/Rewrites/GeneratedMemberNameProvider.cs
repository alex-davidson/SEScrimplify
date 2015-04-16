using System;
using SEScrimplify.Analysis;

namespace SEScrimplify.Rewrites
{
    public class GeneratedMemberNameProvider : IGeneratedMemberNameProvider
    {
        public GeneratedMemberNameProvider() : this(new Random().Next())
        {
        }

        public GeneratedMemberNameProvider(int seed)
        {
            scopeNum = seed;
            methodNum = seed;
        }

        private int scopeNum;
        private int methodNum;

        public string NameLambdaScopeStruct()
        {
            return String.Format("LambdaScope{0}", scopeNum++);
        }
        public string NameLambdaMethod(LambdaDefinition definition)
        {
            return String.Format("{0}{1}", definition.GetRelatedMethodString(), methodNum++);
        }
    }
}