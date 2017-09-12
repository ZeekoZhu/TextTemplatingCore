using System.Collections.Generic;

namespace TextTemplating.Tools
{
    public class ProjectMetadata
    {
        public Dictionary<string, string> Metadatas = new Dictionary<string, string>();

        public string AssemblyName => Metadatas[nameof(AssemblyName)];
        public string OutputPath => Metadatas[nameof(OutputPath)];
        public string RootNamespace => Metadatas[nameof(RootNamespace)];
        public string ProjectDir => Metadatas[nameof(ProjectDir)];
        public string TargetFileName => Metadatas[nameof(TargetFileName)];

    }
}