using System.Collections.Generic;
using System.Linq;
using Roslyn.Compilers;
using Roslyn.Compilers.Common;
using Roslyn.Compilers.CSharp;

namespace DocPlagiarizer
{
    public static class ISymbolExtensions
    {
        public static IEnumerable<SyntaxNode> GetSyntaxNodes(this ISymbol symbol)
        {
            return symbol.DeclaringSyntaxNodes.AsEnumerable().Cast<SyntaxNode>();
        }

        public static IEnumerable<ISymbol> ImplementedInterfaceMember(this ISymbol symbol)
        {
            var type = symbol.ContainingType;
            return type
                .AllInterfaces
                .SelectMany(i => i.GetMembers())
                .ToDictionary(m => m, m => type.FindImplementationForInterfaceMember(m))
                .Where(kvp => kvp.Value == symbol)
                .Select(kvp => kvp.Key);
        }
    }
}