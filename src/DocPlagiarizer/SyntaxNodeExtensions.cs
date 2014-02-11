using System;
using System.Linq;
using Roslyn.Compilers.CSharp;

namespace DocPlagiarizer
{
    public static class SyntaxNodeExtensions
    {
        private static readonly Func<SyntaxTrivia, bool> IsDocumentationComment = t => t.Kind == SyntaxKind.DocumentationCommentTrivia;

        public static string GetDocumentationCommentText(this SyntaxNode node)
        {
            var commentTrivia = node.GetLeadingTrivia().Where(IsDocumentationComment);
            return Syntax.TriviaList(commentTrivia).ToFullString();
        }

        public static SyntaxNode WithDocumentationComment(this SyntaxNode node, string documentationComment)
        {
            var nonCommentTrivia = node.GetLeadingTrivia().WithoutDocumentationComment();
            var indentAdjustedComment = documentationComment.WithIndentation(node.GetIndentation());
            var updatedTrivia = nonCommentTrivia.Concat(Syntax.ParseLeadingTrivia(indentAdjustedComment));
            return node.WithLeadingTrivia(updatedTrivia);
        }

        public static string GetIndentation(this SyntaxNode node)
        {
            var trivia = node.GetLeadingTrivia();
            if (!trivia.Any())
                return String.Empty;

            var lastTrivia = trivia.Last();
            return lastTrivia == null || lastTrivia.IsWhitespace() ? lastTrivia.ToFullString() : String.Empty;
        }
    }
}