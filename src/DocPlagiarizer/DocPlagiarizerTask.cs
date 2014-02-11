using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;
using Roslyn.Services;
using System.Collections.Generic;
using Roslyn.Compilers.Common;
using Roslyn.Services.CSharp.Extensions;
using Roslyn.Services.CodeGeneration;
using System;

namespace DocPlagiarizer
{
    public class DocPlagiarizerTask : Task
    {
        public override bool Execute()
        {
            try
            {
                PullComments();
            }
            catch (Exception ex)
            {
                Log.LogErrorFromException(ex);
            }

            return !Log.HasLoggedErrors;
        }

        private void PullComments()
        {
            var compilation = LoadCurrentCompilation();
            var nodesToComment = GetNodesAndComments(compilation);
            var modifiedTrees = new List<SyntaxTree>();

            var classesToUpdate =
                nodesToComment
                    .Where(n => n.Key is ClassDeclarationSyntax)
                    .Select(n => new
                    {
                        ClassName = (n.Key as ClassDeclarationSyntax).Identifier.ToFullString(),
                        Comment = n.Value
                    })
                    .ToList();

            foreach (var tree in compilation.SyntaxTrees)
            {
                var nodes = tree.GetRoot().DescendantNodes().Where(nodesToComment.ContainsKey).ToList();
                if (!nodes.Any())
                    continue;

                var replacementNodes = nodes.ToDictionary(node => node, node => CreateCommentedNode(node, nodesToComment[node]));
                var classKeys = replacementNodes.Keys.Where(k => k.Kind == SyntaxKind.ClassDeclaration);
                var memberKeys = replacementNodes.Keys.Except(classKeys);
                var cu = tree.GetRoot().ReplaceNodes(memberKeys, (original, rewritten) => replacementNodes[original]);

                var classNodes = cu.DescendantNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .Where(d =>
                    {
                        var className = d.Identifier.ToFullString();
                        return classesToUpdate.Select(c => c.ClassName).Contains(className);
                    })
                    .ToDictionary(n => n,
                        n =>
                            CreateCommentedNode(n,
                                classesToUpdate.Single(c => c.ClassName == n.Identifier.ToFullString()).Comment));

                cu = cu.ReplaceNodes(classNodes.Keys, (original, rewritten) => classNodes[original]);
                modifiedTrees.Add(SyntaxTree.Create(cu, tree.FilePath, tree.Options));
            }

            foreach (var tree in modifiedTrees)
            {
                File.WriteAllText(tree.FilePath, tree.GetText().ToString(), Encoding.UTF8);
            }

            Log.LogMessage("Modified " + modifiedTrees.Count + " files while copying documentation comments.");
        }

        private static SyntaxNode CreateCommentedNode(SyntaxNode node, SyntaxTriviaList documentationComment)
        {
            var existingTrivia = node.GetLeadingTrivia();
            if (node.HasDocumentationComment())
            {
                Predicate<SyntaxTrivia> IsWhitespaceBeforeDocumentationComment = t => t.IsWhitespace() && !t.IsLastInList() && t.NextTrivia().Kind == SyntaxKind.DocumentationCommentTrivia;
                existingTrivia =
                    Syntax.TriviaList(
                        existingTrivia.Where(
                            t =>
                                !IsWhitespaceBeforeDocumentationComment(t) &&
                                t.Kind != SyntaxKind.DocumentationCommentTrivia));
            }

            var existingNonCommentTrivia = existingTrivia.Where(t => t.Kind != SyntaxKind.DocumentationCommentTrivia).ToList();
            var nodeIndent = existingNonCommentTrivia.Last().IsWhitespace() ? existingNonCommentTrivia.Last().ToFullString() : String.Empty;
            var indentAdjustedComment = ReplaceIndentation(documentationComment, nodeIndent);
            var updatedTrivia = existingNonCommentTrivia.Concat(indentAdjustedComment);
            return node.WithLeadingTrivia(updatedTrivia);
        }

        private static SyntaxTriviaList ReplaceIndentation(SyntaxTriviaList trivia, string indentation)
        {
            var triviaString = Regex.Replace(trivia.ToFullString(), @"(\r\n|\n)([\t ]*)", @"$1" + indentation);
            return Syntax.ParseLeadingTrivia(triviaString);
        }

        private Compilation LoadCurrentCompilation()
        {
            var projectFileName = this.BuildEngine.ProjectFileOfTaskNode;
            var project = ProjectCollection.GlobalProjectCollection.LoadProject(projectFileName);
            var outputPath = project.GetProperty("OutputPath").EvaluatedValue;

            if (!Path.IsPathRooted(outputPath))
            {
                outputPath = Path.Combine(Environment.CurrentDirectory, outputPath);
            }

            var searchPaths = ReadOnlyArray.OneOrZero(outputPath);
            var resolver = new DiskFileResolver(searchPaths, searchPaths, Environment.CurrentDirectory, arch => true, System.Globalization.CultureInfo.CurrentCulture);
            
            var metadataFileProvider = new MetadataFileProvider();

            // just grab a list of references (if they're null, ignore)
            var list = project.GetItems("Reference").Select(item =>
            {
                var include = item.EvaluatedInclude;
                var path = resolver.ResolveAssemblyName(include);
                if (path == null) return null;
                return metadataFileProvider.GetReference(path);
            }).Where(x => x != null);

            return Compilation.Create(project.GetPropertyValue("AssemblyName"),
                syntaxTrees: project.GetItems("Compile").Select(c => SyntaxTree.ParseFile(c.EvaluatedInclude)),
                references: list);
        }

        private IDictionary<CommonSyntaxNode, SyntaxTriviaList> GetNodesAndComments(Compilation compilation)
        {
            var meh = new Dictionary<CommonSyntaxNode, SyntaxTriviaList>();
            var typesThatImplementInterfaces = compilation.GetNamedTypes().Where(t => t.AllInterfaces.Any());
            foreach (var type in typesThatImplementInterfaces)
            {
                // Only copy interface comment if we're implementing a single interface
                if (type.AllInterfaces.Count == 1)
                {
                    var face = type.AllInterfaces.Single();
                    if (face.HasDocumentationComment() && face.FindImplementations(compilation).Count() == 1)
                    {
                        var faceNode = face.DeclaringSyntaxNodes.Single() as SyntaxNode;
                        var interfaceComment = faceNode.GetDocumentationComment();
                        foreach (var node in type.DeclaringSyntaxNodes.OfType<SyntaxNode>())
                        {
                            if (!node.HasSameCommentAs(faceNode))
                            {
                                meh.Add(node, interfaceComment);
                            }
                        }
                    }
                }

                foreach (var interfaceMember in type.AllInterfaces.SelectMany(i => i.GetMembers()))
                {
                    var implementation = type.FindImplementationForInterfaceMember(interfaceMember);
                    if (implementation == null) continue;
                    var memberDocumentation = interfaceMember.GetDocumentationComment();
                    if (memberDocumentation == DocumentationComment.Empty)
                        continue;

                    var faceNode = interfaceMember.DeclaringSyntaxNodes.Single() as SyntaxNode; // Expecting an interface member to only ever have one declaration
                    var interfaceComment = faceNode.GetLeadingTrivia().Single(t => t.Kind == SyntaxKind.DocumentationCommentTrivia);

                    foreach (var node in implementation.DeclaringSyntaxNodes.Select(n => n as SyntaxNode))
                    {
                        if (!node.HasSameCommentAs(faceNode))
                            meh.Add(node, interfaceComment);
                    }
                }
            }

            return meh;
        }
    }
}