using System;
using System.Linq;
using Roslyn.Compilers.CSharp;

namespace CustomBuildTasks
{
    public static class SyntaxNodeExtensions
    {
        private static readonly Func<SyntaxTrivia, bool> IsDocumentationComment = t => t.Kind == SyntaxKind.DocumentationCommentTrivia;

        public static bool HasDocumentationComment(this SyntaxNode @this)
        {
            return @this.GetLeadingTrivia().Any(IsDocumentationComment);
        }

        public static SyntaxTriviaList GetDocumentationComment(this SyntaxNode @this)
        {
            if (!@this.GetLeadingTrivia().Any(IsDocumentationComment))
                return Syntax.TriviaList();

            var firstComment = @this.GetLeadingTrivia().First(IsDocumentationComment);
            var lastComment = @this.GetLeadingTrivia().Last(IsDocumentationComment);
            if (firstComment == lastComment)
                return Syntax.TriviaList(firstComment);


            return Syntax.TriviaList(@this.GetLeadingTrivia().Where(IsDocumentationComment));
        }

        public static bool HasSameCommentAs(this SyntaxNode @this, SyntaxNode other)
        {
            if (!@this.HasDocumentationComment() && !other.HasDocumentationComment())
                return true;

            return @this.GetDocumentationComment().ToFullString().WithoutIndentation() == other.GetDocumentationComment().ToFullString().WithoutIndentation();
        }

        public static SyntaxNode WithDocumentationComment(this SyntaxNode @this, SyntaxTrivia documentationComment)
        {
            if (documentationComment.Kind != SyntaxKind.DocumentationCommentTrivia)
                throw new ArgumentException("documentationComment is not of kind DocumentationCommentTrivia");

            var existingTrivia = @this.GetLeadingTrivia();
            if (existingTrivia.Any(t => t.Kind == SyntaxKind.DocumentationCommentTrivia))
            {
                var existingComment = existingTrivia.Single(t => t.Kind == SyntaxKind.DocumentationCommentTrivia);
                var indexOfComment = existingTrivia.IndexOf(existingComment);
                if (indexOfComment > 0)
                {
                    var triviaBeforeComment = existingTrivia.ElementAt(indexOfComment - 1);
                    if (triviaBeforeComment.Kind == SyntaxKind.WhitespaceTrivia)
                    {
                        existingTrivia = Syntax.TriviaList(existingTrivia.Where((t, i) => i != indexOfComment - 1));
                    }
                }
            }

            var existingNonCommentTrivia = existingTrivia.Where(t => t.Kind != SyntaxKind.DocumentationCommentTrivia);
            var currentIndent = existingNonCommentTrivia.Last().ToFullString();
            var indentAdjustedComment = documentationComment.WithIndentation(currentIndent);
            var updatedTrivia = existingNonCommentTrivia.Concat(indentAdjustedComment);
            return @this.WithLeadingTrivia(updatedTrivia);
        }
    }
}