using System;
using System.IO;

namespace TextTemplating.Tools
{
    public class ProjectNotFoundException : FileNotFoundException
    {
        public ProjectNotFoundException(string message) : base(message)
        {}
    }
}