using System;
using System.Collections.Generic;
using System.Text;

namespace TextTemplating.Tools
{
    interface IMetadataResolveable
    {
        /// <summary>
        /// Get project metadata from project file
        /// </summary>
        /// <param name="projectFilePath"></param>
        /// <returns></returns>
        ProjectMetadata ReadProject(string projectFilePath);
        /// <summary>
        /// Get location of all the assemblies that the project referenced
        /// </summary>
        /// <param name="metadata">Project metadata</param>
        /// <returns></returns>
        List<string> GetReferencedAssembliesLocaltion(ProjectMetadata metadata);
    }
}
