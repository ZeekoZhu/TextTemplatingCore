using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace TextTemplating.Infrastructure
{
    public interface IMetadataResolveable
    {
        ProjectMetadata ProjectMetadata { get; set; }

        /// <summary>
        /// Get project metadata from project file
        /// </summary>
        /// <param name="projectFilePath"></param>
        /// <returns></returns>
        ProjectMetadata ReadProject(string projectFilePath);


        /// <summary>
        /// Resolve all MetadataReference that the project contains for compiling
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns></returns>
        List<MetadataReference> ResolveMetadataReference(ProjectMetadata metadata = null);
    }
}
