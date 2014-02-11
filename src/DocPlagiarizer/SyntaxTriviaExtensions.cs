using System;
using System.Linq;
using System.Text.RegularExpressions;
using Roslyn.Compilers.CSharp;

namespace DocPlagiarizer
{
    public static class SyntaxTriviaExtensions
    {

        public static bool IsWhitespace(this SyntaxTrivia trivia)
        {
            return trivia != null && trivia.Kind == SyntaxKind.WhitespaceTrivia;
        }

        public static SyntaxTriviaList GetTriviaList(this SyntaxTrivia trivia)
        {
            var triviaList = default(SyntaxTriviaList);

            if (trivia.Token.LeadingTrivia.Contains(trivia))
            {
                triviaList = trivia.Token.LeadingTrivia;
            }
            if (trivia.Token.TrailingTrivia.Contains(trivia))
            {
                triviaList = trivia.Token.TrailingTrivia;
            }

            if (triviaList.Contains(trivia))
                return triviaList;

            throw new Exception("This should never happen. Unable to find TriviaList for Trivia");
        }

        public static bool IsLastInList(this SyntaxTrivia trivia)
        {
            return trivia.GetTriviaList().Last() == trivia;
        }

        public static SyntaxTrivia NextTrivia(this SyntaxTrivia trivia)
        {
            if (trivia.IsLastInList())
                throw new InvalidOperationException("Cannot get next trivia when at the end of list. Call IsLastInList to check if there are more trivia tokens before calling this method.");

            var triviaList = trivia.GetTriviaList();
            return triviaList.ElementAt(triviaList.IndexOf(trivia) + 1);
        }

        public static bool IsDocumentationComment(this SyntaxTrivia trivia)
        {
            return trivia.Kind == SyntaxKind.DocumentationCommentTrivia;
        }
    }
}