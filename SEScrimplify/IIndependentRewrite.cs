using Microsoft.CodeAnalysis;

namespace SEScrimplify
{
    /// <summary>
    /// Performs an independent rewrite of the entire tree. This rewrite cannot be batched
    /// with others.
    /// </summary>
    public interface IIndependentRewrite
    {
        SyntaxTree Rewrite(SyntaxTree tree, IScriptCompiler compiler);
    }
}