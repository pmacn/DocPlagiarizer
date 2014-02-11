using System.Linq;
using Roslyn.Compilers.CSharp;

namespace DocPlagiarizer
{
    public class DocumentationRewriter : SyntaxRewriter
    {
        readonly SemanticModel semanticModel;

        public DocumentationRewriter(SemanticModel model)
        {
            semanticModel = model;
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            var symbol = semanticModel.GetDeclaredSymbol(node) as NamedTypeSymbol;
            var visitedNode = base.VisitClassDeclaration(node) as ClassDeclarationSyntax;

            if (symbol == null || symbol.AllInterfaces.Count != 1)
                return visitedNode;

            var face = symbol.AllInterfaces.Single();
            if (symbol.GetDocumentationComment().Equals(face.GetDocumentationComment()))
                return visitedNode;

            var facenode = face.DeclaringSyntaxNodes.Single();
            return visitedNode.WithDocumentationComment(facenode.GetDocumentationCommentText());
        }

        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            base.VisitMethodDeclaration(node);
            return VisitMemberDeclaration(node);
        }

        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            base.VisitPropertyDeclaration(node);
            return VisitMemberDeclaration(node);
        }

        public override SyntaxNode VisitEventFieldDeclaration(EventFieldDeclarationSyntax node)
        {
            base.VisitEventFieldDeclaration(node);
            return VisitMemberDeclaration(node);
        }
        private SyntaxNode VisitMemberDeclaration(MemberDeclarationSyntax node)
        {
            if (node.Parent is InterfaceDeclarationSyntax)
                return node;

            var symbol =
                node is EventFieldDeclarationSyntax ?
                semanticModel.GetDeclaredSymbol((node as EventFieldDeclarationSyntax).Declaration.Variables[0]) :
                semanticModel.GetDeclaredSymbol(node);

            var type = symbol.ContainingType;
            if (type == null)
                return node;

            var faceMembers = symbol.ImplementedInterfaceMember();
            if (faceMembers.Count() != 1)
                return node;

            var facenode = node is EventFieldDeclarationSyntax ?
                faceMembers.Single().GetSyntaxNodes().Single().Parent.Parent :
                faceMembers.Single().GetSyntaxNodes().Single();
            if (node.GetDocumentationCommentText().WithoutIndentation() == facenode.GetDocumentationCommentText().WithoutIndentation())
                return node;

            return node.WithDocumentationComment(facenode.GetDocumentationCommentText());
        }
    }
}
