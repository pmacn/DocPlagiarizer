using Roslyn.Compilers;
using System;
using System.IO;
using System.Linq;

namespace CustomBuildTasks
{
    public class DiskFileResolver : FileResolver
    {
        public DiskFileResolver(ReadOnlyArray<string> assemblySearchPaths, ReadOnlyArray<string> keyFileSearchPaths, string baseDirectory, Func<System.Reflection.ProcessorArchitecture, bool> architectureFilter, System.Globalization.CultureInfo preferredCulture)
            : base(assemblySearchPaths, keyFileSearchPaths, baseDirectory, architectureFilter, preferredCulture)
        {

        }

        public override string ResolveAssemblyName(string displayName)
        {
            var items = this.AssemblySearchPaths.SelectMany(
                p => Directory.GetFiles(p, "*.dll", SearchOption.AllDirectories))
                    .Select(f => System.Reflection.AssemblyName.GetAssemblyName(f))
                    .Distinct()
                    .ToList();

            var firstMatch = items
                    .FirstOrDefault(n => n.Name == displayName);

            if (firstMatch != null)
                return firstMatch.EscapedCodeBase.Replace("file:///", "");

            return base.ResolveAssemblyName(displayName);
        }

        public override string ResolveMetadataFile(string path, string baseFilePath)
        {
            return base.ResolveMetadataFile(path, baseFilePath);
        }

        public override string ResolveStrongNameKeyFile(string path, string baseFilePath)
        {
            return base.ResolveStrongNameKeyFile(path, baseFilePath);
        }
    }
}
