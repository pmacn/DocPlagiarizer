using System.Collections.Generic;
using System.Linq;
using Roslyn.Compilers.Common;
using Roslyn.Compilers.CSharp;

namespace DocPlagiarizer
{
    public static class CompilationExtensions
    {
        public static IReadOnlyList<INamedTypeSymbol> GetNamedTypes(this Compilation @this)
        {
            var assemblyName = @this.Assembly.Name;
            return GetAllTypesInNamespace(@this.GlobalNamespace).Where(t => t.ContainingAssembly.Name == assemblyName).ToList().AsReadOnly();
        }

        private static IEnumerable<INamedTypeSymbol> GetAllTypesInNamespace(INamespaceOrTypeSymbol namespaceSymbol)
        {
            foreach (var member in namespaceSymbol.GetMembers())
            {
                if (member is INamedTypeSymbol)
                    yield return (INamedTypeSymbol)member;

                if (member is INamespaceOrTypeSymbol)
                {
                    foreach (var type in GetAllTypesInNamespace((INamespaceOrTypeSymbol)member))
                    {
                        yield return type;
                    }
                }
            }
        }
    }
}