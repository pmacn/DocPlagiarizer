using System.Linq;
using Roslyn.Compilers.Common;
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
            var symbol = semanticModel.GetDeclaredSymbol(node);
            var visitedNode = base.VisitClassDeclaration(node);
            return VisitTypeDeclaration(visitedNode as TypeDeclarationSyntax, symbol);
        }

        public override SyntaxNode VisitEventDeclaration(EventDeclarationSyntax node)
        {
            base.VisitEventDeclaration(node);
            if (node.Parent is InterfaceDeclarationSyntax)
                return node;

            var symbol = semanticModel.GetDeclaredSymbol(node);

            var type = symbol.ContainingType;
            if (type == null)
                return node;

            var faceMembers = symbol.ImplementedInterfaceMember();
            if (faceMembers.Count() != 1)
                return node;

            var interfaceNode = faceMembers.Single().GetSyntaxNodes().Single().Parent.Parent;
            if (interfaceNode.GetDocumentationCommentText() == node.GetDocumentationCommentText())
                return node;

            return node.WithDocumentationComment(interfaceNode.GetDocumentationCommentText());
        }

        public override SyntaxNode VisitEventFieldDeclaration(EventFieldDeclarationSyntax node)
        {
            base.VisitEventFieldDeclaration(node);

            if (node.Parent is InterfaceDeclarationSyntax)
                return node;

            var symbol = semanticModel.GetDeclaredSymbol((node as EventFieldDeclarationSyntax).Declaration.Variables[0]);

            var type = symbol.ContainingType;
            if (type == null)
                return node;

            var faceMembers = symbol.ImplementedInterfaceMember();
            if (faceMembers.Count() != 1)
                return node;

            var facenode = faceMembers.Single().GetSyntaxNodes().Single().Parent.Parent;
            if (node.GetDocumentationCommentText().WithoutIndentation() == facenode.GetDocumentationCommentText().WithoutIndentation())
                return node;

            return node.WithDocumentationComment(facenode.GetDocumentationCommentText());
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

        public override SyntaxNode VisitStructDeclaration(StructDeclarationSyntax node)
        {
            var symbol = semanticModel.GetDeclaredSymbol(node);
            var visitedNode = base.VisitStructDeclaration(node);
            return VisitTypeDeclaration(visitedNode as TypeDeclarationSyntax, symbol);
        }

        private SyntaxNode VisitMemberDeclaration(MemberDeclarationSyntax node)
        {
            if (node.Parent is InterfaceDeclarationSyntax)
                return node;

            var symbol = semanticModel.GetDeclaredSymbol(node);

            var type = symbol.ContainingType;
            if (type == null)
                return node;

            var faceMembers = symbol.ImplementedInterfaceMember();
            if (faceMembers.Count() != 1)
                return node;

            var interfaceMember = faceMembers.Single();
            if (interfaceMember.GetDocumentationComment().Equals(symbol.GetDocumentationComment()))
                return node;

            return node.WithDocumentationComment(interfaceMember.GetSyntaxNodes().Single().GetDocumentationCommentText());
        }

        private SyntaxNode VisitTypeDeclaration(TypeDeclarationSyntax node, INamedTypeSymbol symbol)
        {
            if (node == null || symbol == null || symbol.AllInterfaces.Count != 1)
                return node;

            var face = symbol.AllInterfaces.Single();
            if (symbol.GetDocumentationComment().Equals(face.GetDocumentationComment()))
                return node;

            var facenode = face.GetSyntaxNodes().Single();
            return node.WithDocumentationComment(facenode.GetDocumentationCommentText());
        }
    }
}
