using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace SEScrimplify
{
    public interface IScriptCompiler
    {
        CSharpCompilation Compile(SyntaxTree tree);
        CSharpCompilation StrictCompile(SyntaxTree tree);
    }
}