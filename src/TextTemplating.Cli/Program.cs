using System;
using System.IO;
using Microsoft.Extensions.CommandLineUtils;
using TextTemplating.Infrastructure;

namespace TextTemplating.Tools
{
    class Program
    {
        static int Main(string[] args)
        {
            const string helpTemplate = "-h|--help";
            var app = new CommandLineApplication
            {
                Description = "A simple Text Template Transformer for .Net Core"
            };
            var process = app.Command("proc", AppCommands.Process);
            app.HelpOption(helpTemplate);
            try
            {
                return app.Execute(args);
            }
            catch (CommandParsingException e)
            {
                app.ShowHelp(e.Command.Name);
                return 1;
            }
        }


    }
}

