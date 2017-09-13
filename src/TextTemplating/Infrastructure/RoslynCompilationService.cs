using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;

namespace TextTemplating.Infrastructure
{
    public class RoslynCompilationService
    {
        private readonly ConcurrentDictionary<string, AssemblyMetadata> _metadataFileCache = new ConcurrentDictionary<string, AssemblyMetadata>(StringComparer.OrdinalIgnoreCase);

        private readonly ITextTemplatingEngineHost _host;
        private readonly IMetadataResolveable _resolver;

        public RoslynCompilationService(ITextTemplatingEngineHost host, IMetadataResolveable resolver)
        {
            _host = host;
            _resolver = resolver;
        }

        public Assembly Compile(string assemblyName, PreprocessResult preprocessResult)
        {
            var references = new List<MetadataReference>();
            // project references
            references.AddRange(_resolver.ResolveMetadataReference());
            // assembly instruction
            references.AddRange(preprocessResult.References.Select(ResolveAssemblyReference)
                .Where(metadata => metadata != null));

            var compilation = CSharpCompilation.Create(
                assemblyName,
                new[] { SyntaxFactory.ParseSyntaxTree(preprocessResult.PreprocessedContent) },
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using (var stream = new MemoryStream())
            {
                var result = compilation.Emit(stream);
                _host.LogErrors(result);

                var transformationAssembly = Assembly.Load(stream.ToArray());

                //var transformationAssembly = (Assembly)typeof(Assembly).GetTypeInfo().GetDeclaredMethods("Load").First(m =>
                //{
                //    var parameters = m.GetParameters();
                //    return parameters.Length == 1 && parameters[0].ParameterType == typeof(byte[]);
                //}).Invoke(null, new[] { stream.ToArray() });

                return transformationAssembly;
            }
        }

        /// <summary>
        /// Resolve assembly reference by .dll file's absolute path
        /// </summary> 
        /// <param name="assemblyReference">assembly location</param>
        /// <returns></returns>
        public MetadataReference ResolveAssemblyReference(string assemblyReference)
        {
            var projectRegex = new Regex(@"$(ProjectDir)");
            var absolutePath = projectRegex.Replace(assemblyReference, _resolver.ProjectMetadata.ProjectDir);
            return File.Exists(absolutePath) ? MetadataReference.CreateFromFile(absolutePath) : null;
        }
    }
}