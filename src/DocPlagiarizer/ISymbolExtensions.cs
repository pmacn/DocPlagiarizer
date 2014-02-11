using System.Linq;
using Roslyn.Compilers;
using Roslyn.Compilers.Common;
using Roslyn.Compilers.CSharp;
using System.Collections.Generic;

namespace DocPlagiarizer
{
    public static class ISymbolExtensions
    {
        public static bool HasDocumentationComment(this ISymbol @this)
        {
            return @this.GetDocumentationComment() != DocumentationComment.Empty;
        }

        public static IEnumerable<INamedTypeSymbol> FindImplementations(this INamedTypeSymbol symbol, Compilation compilation)
        {
            return compilation.GetNamedTypes().Where(t => t.AllInterfaces.Contains(symbol));
        }
    }
}