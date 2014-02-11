using System;
using System.Linq;
using Roslyn.Compilers.CSharp;

namespace DocPlagiarizer
{
    public static class SyntaxTriviaListExtensions
    {
        static readonly Predicate<SyntaxTrivia> IsWhitespaceBeforeDocumentationComment =
            t => t.IsWhitespace() && !t.IsLastInList() && t.NextTrivia().IsDocumentationComment();

        public static SyntaxTriviaList WithoutDocumentationComment(this SyntaxTriviaList list)
        {
            return Syntax.TriviaList(list.Where(t => !IsWhitespaceBeforeDocumentationComment(t) && !t.IsDocumentationComment()));
        }
    }
}