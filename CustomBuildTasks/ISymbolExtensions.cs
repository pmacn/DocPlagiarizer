using Roslyn.Compilers;
using Roslyn.Compilers.Common;

namespace CustomBuildTasks
{
    public static class ISymbolExtensions
    {
        public static bool HasDocumentationComment(this ISymbol @this)
        {
            return @this.GetDocumentationComment() != DocumentationComment.Empty;
        }
    }
}