using System.Collections.Generic;
using System.Linq;
using Roslyn.Compilers.CSharp;

namespace CustomBuildTasks
{
    public static class SyntaxTreeExtensions
    {
        public static bool ContainsAny(this SyntaxTree @this, IEnumerable<SyntaxNode> nodes)
        {
            return @this.GetRoot().DescendantNodes().Any(nodes.Contains);
        }
    }
}