using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roslyn.Compilers.CSharp;

namespace DocPlagiarizer
{
    public class ClassRewriter : SyntaxRewriter
    {
        readonly SemanticModel semanticModel;

        public ClassRewriter(SemanticModel model)
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

        public override SyntaxNode VisitEventDeclaration(EventDeclarationSyntax node)
        {
            return VisitMemberDeclaration(base.VisitEventDeclaration(node) as EventDeclarationSyntax);
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
            if (symbol.GetDocumentationComment().Equals(interfaceMember.GetDocumentationComment()))
                return node;

            return node.WithDocumentationComment(interfaceMember.GetSyntaxNodes().Single().GetDocumentationCommentText());
        }
    }
}
