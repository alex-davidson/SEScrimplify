using System;
using Microsoft.CodeAnalysis;
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
            fieldNum = seed;
            methodNum = seed;
            iteratorNum = seed;
        }

        private int scopeNum;
        private int fieldNum;
        private int methodNum;
        private int iteratorNum;

        public string NameLambdaScopeStruct()
        {
            return String.Format("LambdaScope{0}", scopeNum++);
        }
        public string NameLambdaMethod(LambdaModel model)
        {
            return String.Format("{0}{1}", model.GetRelatedMethodString(), methodNum++);
        }


        public string NameLambdaScopeField(ISymbol symbol)
        {
            return symbol.Name + fieldNum++;
        }


        public string NameIterator(ISymbol iteratee)
        {
            return String.Format("{0}Iterator{1}", iteratee.Name, iteratorNum++);
        }
    }
}