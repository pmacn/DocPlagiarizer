using Roslyn.Compilers;
using System;
using System.IO;
using System.Linq;

namespace DocPlagiarizer
{
    public class DiskFileResolver : FileResolver
    {
        public DiskFileResolver(ReadOnlyArray<string> assemblySearchPaths, ReadOnlyArray<string> keyFileSearchPaths, string baseDirectory, Func<System.Reflection.ProcessorArchitecture, bool> architectureFilter, System.Globalization.CultureInfo preferredCulture)
            : base(assemblySearchPaths, keyFileSearchPaths, baseDirectory, architectureFilter, preferredCulture)
        {

        }

        public override string ResolveAssemblyName(string displayName)
        {
            var items = this.AssemblySearchPaths
                .Where(dir => Directory.Exists(dir))
                .SelectMany(dir => Directory.GetFiles(dir, "*.dll", SearchOption.AllDirectories))
                .Select(file => System.Reflection.AssemblyName.GetAssemblyName(file))
                .Distinct();
            
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
