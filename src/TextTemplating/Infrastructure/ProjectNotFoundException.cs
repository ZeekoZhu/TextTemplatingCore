using System.IO;

namespace TextTemplating.Infrastructure
{
    public class ProjectNotFoundException : FileNotFoundException
    {
        public ProjectNotFoundException(string message) : base(message)
        {}
    }
}