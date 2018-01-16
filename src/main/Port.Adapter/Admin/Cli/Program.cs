using Microsoft.Extensions.Configuration;
using Nancy.Bootstrapper;
using Nancy.Hosting.Self;
using org.neurul.Common;
using org.neurul.Common.Domain.Model;
using org.neurul.Common.Http.Cli;
using System;
using System.IO;

namespace org.neurul.Cortex.Port.Adapter.Admin.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddEnvironmentVariables();

                IConfigurationRoot configuration = builder.Build();

                var settings = new Settings();
                configuration.Bind(settings);
                
                AssertionConcern.AssertPathValid(settings.DatabasePath, nameof(settings.DatabasePath));

                MultiHostProgram.Start(
                    new DefaultConsoleWrapper(),
                    "Neurul Cortex",
                    args,
                    new string[] { "In", "Out" },
                    new INancyBootstrapper[] {
                        new In.Http.CustomBootstrapper(settings.DatabasePath),
                        new Out.Http.CustomBootstrapper()
                    }
                    );
            }
            catch (Exception ex)
            {
                Console.WriteLine("The following exception occurred:");
                Console.WriteLine();
                Console.WriteLine(ex.Message);
                Console.WriteLine();
                Console.WriteLine("Press any key to continue...");
                Console.ReadLine();
            }
        }
    }
}
