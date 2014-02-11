using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Utilities;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;

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
            var project = LoadCurrentProject();
            var compilation = CreateCompilation(project);

            var modifiedTrees = compilation.SyntaxTrees
                .Select(t =>
                {
                    var oldRoot = t.GetRoot();
                    var newRoot = new ClassRewriter(compilation.GetSemanticModel(t))
                        .Visit(oldRoot);

                    return newRoot.EquivalentTo(oldRoot) ?
                        null :
                        SyntaxTree.Create((CompilationUnitSyntax)newRoot, t.FilePath, t.Options);
                })
                .Where(t => t != null)
                .ToArray();

            foreach (var tree in modifiedTrees)
            {
                File.WriteAllText(tree.FilePath, tree.GetText().ToString(), Encoding.UTF8);
            }

            Log.LogMessage(String.Format("Modified {0} files while copying documentation comments.", modifiedTrees.Count()));
        }

        private Project LoadCurrentProject()
        {
            var projectFileName = this.BuildEngine.ProjectFileOfTaskNode;
            return ProjectCollection.GlobalProjectCollection.LoadProject(projectFileName);
        }

        private Compilation CreateCompilation(Project project)
        {
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
    }
}