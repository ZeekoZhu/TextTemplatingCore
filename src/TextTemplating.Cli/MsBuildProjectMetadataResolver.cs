using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using TextTemplating.Infrastructure;

namespace TextTemplating.Tools
{
    /// <summary>
    /// Using MSBuild to get project metadata
    /// </summary>
    class MsBuildProjectMetadataResolver : IMetadataResolveable
    {
        public ProjectMetadata ProjectMetadata { get; set; }
        private List<string> ReferencesPath { get; set; }

        /// <inheritdoc />
        public ProjectMetadata ReadProject(string projectFilePath)
        {
            // prepare necessary files and directories
            string projectRoot = Path.GetDirectoryName(projectFilePath);
            string projectFile = Path.GetFileName(projectFilePath);
            string objDir = Path.Combine(projectRoot, "obj");
            string targetFilePath = Path.Combine(objDir, projectFile + ".TTGetMetadata.targets");
            Directory.CreateDirectory(objDir);
            string projectName = Assembly.GetExecutingAssembly().GetName().Name;
            using (var targets = typeof(MsBuildProjectMetadataResolver).Assembly.GetManifestResourceStream($"{projectName}.Resources.TTGetMetadata.targets"))
            using (var outputFile = File.OpenWrite(targetFilePath))
            {
                targets.CopyTo(outputFile);
            }
            string metadataFile = Path.Combine(objDir, $"{projectFile}metadata");
            // build with TTGetMetadata.targets

            // dotnet msbuild /t:TTGetMetadata /p:TTProjectMetadataFile="pathToResult"
            var args = new List<string>
            {
                "msbuild",
                "/t:TTGetMetadata",
                $"/p:TTProjectMetadataFile=\"{metadataFile}\"",
                "/verbosity:quiet",
                "/nologo"
            };
            var processStartInfo = new ProcessStartInfo("dotnet", string.Join(' ', args))
            {
                WorkingDirectory = projectRoot,
                UseShellExecute = false
            };
            var process = Process.Start(processStartInfo);
            process.WaitForExit();

            // reading generated metadata file

            var metadatas = File.ReadAllLines(metadataFile)
                .Select(line => line.Split(": "))
                .Where(segs => segs.Length == 2)
                .Select(segments => new KeyValuePair<string, string>(segments[0], segments[1]));

            File.Delete(metadataFile);
            var projectMetadata = new ProjectMetadata
            {
                Metadatas = new Dictionary<string, string>(metadatas)
            };

            projectMetadata.ProjectFile = projectFile;
            ProjectMetadata = projectMetadata;
            return projectMetadata;
        }


        /// <inheritdoc />
        public List<MetadataReference> ResolveMetadataReference(ProjectMetadata metadata = null)
        {
            metadata = metadata ?? ProjectMetadata;

            var targetName = "ResolveReferencePath.targets";
            var outputPath = Path.Combine(metadata.ProjectDir, "obj", "Debug");
            var referenceTargetPath = Path.Combine(metadata.ProjectDir, "obj",
                $"{metadata.ProjectFile}.{targetName}");
            var referencesFile = Path.Combine(metadata.ProjectDir, "obj", metadata.TargetFileName + ".references");

            // Force CoreCompile targets to execute
            if (Directory.Exists(outputPath))
            {
                Directory.Delete(outputPath, true);
            }

            if (File.Exists(referencesFile))
            {
                File.Delete(referencesFile);
            }

            // copy targets file to obj directory
            string projectName = Assembly.GetExecutingAssembly().GetName().Name;
            using (var targets = typeof(MsBuildProjectMetadataResolver).Assembly.GetManifestResourceStream($"{projectName}.Resources.{targetName}"))
            using (var outputFile = File.OpenWrite(referenceTargetPath))
            {
                targets.CopyTo(outputFile);
            }

            // rebuild and generate reference list
            var arg = "msbuild /v:q /nologo";
            var processStartInfo = new ProcessStartInfo("dotnet", arg)
            {
                WorkingDirectory = metadata.ProjectDir,
                UseShellExecute = false
            };
            var process = Process.Start(processStartInfo);
            process.WaitForExit();

            // read references path
            var lines = File.ReadAllLines(referencesFile)
                .Where(line => string.IsNullOrWhiteSpace(line) == false).ToList();
            lines.Add(Path.Combine(metadata.ProjectDir, metadata.OutputPath, metadata.TargetFileName));
            var metadataReferences = lines
                .Select(path => MetadataReference.CreateFromFile(path) as MetadataReference)
                .ToList();
            File.Delete(referencesFile);


            // load unreferenced assembly so that I can use them to run the compiled assembly
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                var name = new AssemblyName(args.Name).Name + ".dll";
                var assemblyPath = lines.FirstOrDefault(path => path.IndexOf(name, StringComparison.Ordinal) >= 0);
                return assemblyPath == null ? null : Assembly.LoadFrom(assemblyPath);
            };



            return metadataReferences;
        }
    }
}
