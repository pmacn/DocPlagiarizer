using System;
using System.Linq;
using System.Text.RegularExpressions;
using Roslyn.Compilers.CSharp;

namespace DocPlagiarizer
{
    public static class SyntaxTriviaExtensions
    {
        public static SyntaxTriviaList WithIndentation(this SyntaxTrivia @this, string indentation)
        {
            if (indentation == null)
                throw new ArgumentNullException("indentation");

            if (!String.IsNullOrWhiteSpace(indentation))
                throw new ArgumentException("indentation must be empty or whitespace only");

            var triviaString = Regex.Replace(@this.ToFullString(), @"(\r\n|\n)([\t ]*)", @"$1" + indentation);
            return Syntax.ParseLeadingTrivia(triviaString);
        }

        public static bool IsWhitespace(this SyntaxTrivia @this)
        {
            return @this.Kind == SyntaxKind.WhitespaceTrivia;
        }

        public static SyntaxTriviaList GetTriviaList(this SyntaxTrivia @this)
        {
            var triviaList = default(SyntaxTriviaList);

            if (@this.Token.LeadingTrivia.Contains(@this))
            {
                triviaList = @this.Token.LeadingTrivia;
            }
            if (@this.Token.TrailingTrivia.Contains(@this))
            {
                triviaList = @this.Token.TrailingTrivia;
            }

            if (!triviaList.Contains(@this))
                throw new Exception("This should never happen. Unable to find TriviaList for Trivia");

            return triviaList;
        }

        public static bool IsFirstInList(this SyntaxTrivia @this)
        {
            return @this.GetTriviaList().First() == @this;
        }

        public static bool IsLastInList(this SyntaxTrivia @this)
        {
            return @this.GetTriviaList().Last() == @this;
        }

        public static SyntaxTrivia NextTrivia(this SyntaxTrivia @this)
        {
            if (@this.IsLastInList())
                throw new InvalidOperationException("Cannot get next trivia when at the end of list. Call IsLastInList to check if there are more trivia tokens before calling this method.");
            
            var triviaList = @this.GetTriviaList();
            return triviaList.ElementAt(triviaList.IndexOf(@this) + 1);
        }

        public static bool ComesBeforeDocumentationComment(this SyntaxTrivia @this)
        {
            return @this.NextTrivia().Kind == SyntaxKind.DocumentationCommentTrivia;
        }
    }
}