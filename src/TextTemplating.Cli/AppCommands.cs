using System;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.CommandLineUtils;
using TextTemplating.Infrastructure;

namespace TextTemplating.Tools
{
    public static class AppCommands
    {
        const string HelpTemplate = "-h|--help";

        /// <summary>
        /// Find the projet that contains the file
        /// </summary>
        /// <param name="childFile"></param>
        /// <returns></returns>
        static bool TryFindProjectFile(string childFile, out string projectFile)
        {
            string directory = Path.GetDirectoryName(childFile);
            while (Directory.Exists(directory) && Directory.GetDirectoryRoot(directory) != directory)
            {
                var files = Directory.EnumerateFiles(directory, "*.*proj").ToList();
                if (files.Any())
                {
                    projectFile = files.First();
                    return true;
                }
                directory = Path.GetDirectoryName(directory);
            }
            projectFile = null;
            return false;
        }

        #region Process

        public static void Process(CommandLineApplication command)
        {
            command.Description = "Process template to CSharp class file for runtime transform";
            var fileOption = command.Option("-f|--file", "The texttemplate to be processed", CommandOptionType.SingleValue);
            var outputOption = command.Option("-o|--output", "Output directory path, default: out.cs", CommandOptionType.SingleValue);
            var classNameOption = command.Option("-c|--class", "Generated class name, default: GeneratedClass", CommandOptionType.SingleValue);
            var namespaceNameOption = command.Option("-ns|--namespace", "Generated namespace name, default: GeneratedNamespace",
                CommandOptionType.SingleValue);
            command.HelpOption(HelpTemplate);
            command.OnExecute(() =>
            {
                var filePath = Path.Combine(Environment.CurrentDirectory, fileOption.Value());
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var outputPath = Path.Combine(Environment.CurrentDirectory, outputOption.Value() ?? fileName + ".cs");
                if (TryFindProjectFile(filePath, out string projectFile) == false)
                {
                    throw new ProjectNotFoundException("Current work directory is not in a project directory");
                }

                // Resolve metadata
                var resolver = new MsBuildProjectMetadataResolver();
                var metadata = resolver.ReadProject(projectFile);
                

                string className = classNameOption.Value() ?? Path.GetFileName(fileName);
                string namespaceName = namespaceNameOption.Value() ?? metadata.RootNamespace ?? "GeneratedNameSpace";
                try
                {
                    return PreprocessTemplate(filePath, outputPath, className, namespaceName);
                }
                catch (IOException e)
                {
                    switch (e)
                    {
                        case FileNotFoundException fe:
                        case DirectoryNotFoundException de:
                            Console.WriteLine(e.Message);
                            break;
                    }
                    return 1;
                }
            });
        }
        /// <summary>
        /// 转换模板
        /// </summary>
        /// <param name="file">模板文件绝对路径</param>
        /// <param name="outPut">输出路径</param>
        /// <param name="className">生成的类名</param>
        /// <param name="namespaceName">生成的名称空间名</param>
        /// <returns></returns>
        static int PreprocessTemplate(string file, string outPut, string className, string namespaceName)
        {
            var templatesRoot = Path.GetDirectoryName(file);
            var host = new CommandLineEngineHost(templatesRoot);
            var engin = new Engine(host);
            var templateContent = File.ReadAllText(file);
            var result = engin.PreprocessT4Template(templateContent, className, namespaceName);
            File.WriteAllText(outPut, result.PreprocessedContent);
            return 0;
        }

        #endregion
    }
}