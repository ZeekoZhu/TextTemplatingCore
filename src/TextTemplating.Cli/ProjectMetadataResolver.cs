using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TextTemplating.Tools
{
    class MsBuildProjectMetadataResolver : IMetadataResolveable
    {
        public ProjectMetadata ReadProject(string projectFilePath)
        {
            // prepare necessary files and directories
            string projectRoot = Path.GetDirectoryName(projectFilePath);
            string projectFile = Path.GetFileName(projectFilePath);
            string objDir = Path.Combine(projectRoot, "obj");
            string targetFilePath = Path.Combine(objDir, projectFile, ".TTMetadata.targets");
            Directory.CreateDirectory(objDir);
            using (var targets = typeof(ProjectMetadata).Assembly.GetManifestResourceStream("TTGetMetadata.targets"))
            using (var outputFile = File.OpenWrite(targetFilePath))
            {
                targets.CopyTo(outputFile);
            }
            string metadataFile = Path.Combine(objDir, $"{DateTime.Now:G}{projectFile}metadata");
            // build with TTGetMetadata.targets

            // dotnet msbuild /t:TTGetMetadata /p:TTProjectMetadataFile="pathToResult"
            var args = new List<string>
            {
                "msbuild",
                "/t:GetTTProjectMetadata",
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

            var metadatas = File.ReadAllLines(metadataFile).Select(line =>
            {
                var segments = line.Split(':');
                return new KeyValuePair<string, string>(segments[0], segments[1]);
            });
            File.Delete(metadataFile);
            var projectMetadata = new ProjectMetadata
            {
                Metadatas = new Dictionary<string, string>(metadatas)
            };

            return projectMetadata;

        }

        /// <inheritdoc />
        public List<string> GetReferencedAssembliesLocaltion(ProjectMetadata metadata)
        {
            var outputAssemblyPath = Path.Combine(metadata.ProjectDir, metadata.OutputPath, metadata.TargetFileName);
            var assembly = Assembly.LoadFile(outputAssemblyPath);
            return assembly
                    .GetReferencedAssemblies()
                    .Select(referenced => Assembly.Load(referenced).Location).ToList();

        }
    }
}
