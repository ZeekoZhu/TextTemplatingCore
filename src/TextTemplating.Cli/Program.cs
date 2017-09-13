using System;
using System.IO;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using TextTemplating.Infrastructure;

namespace TextTemplating.Tools
{
    class Program
    {
        public static IServiceProvider DI;

        static void ConfigureServices()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IMetadataResolveable, MsBuildProjectMetadataResolver>();
            services.AddSingleton<ITextTemplatingEngineHost, CommandLineEngineHost>();
            services.AddSingleton<RoslynCompilationService>();
            services.AddSingleton<Engine>();
            DI = services.BuildServiceProvider();
        }

        static int Main(string[] args)
        {
            ConfigureServices();
            const string helpTemplate = "-h|--help";
            var app = new CommandLineApplication
            {
                Description = "A simple Text Template Transformer for .Net Core"
            };
            var process = app.Command("proc", AppCommands.ProcessCommand);
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

