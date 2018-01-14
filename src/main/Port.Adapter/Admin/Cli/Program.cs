using Nancy.Bootstrapper;
using Nancy.Hosting.Self;
using org.neurul.Common;
using org.neurul.Common.Domain.Model;
using org.neurul.Common.Http.Cli;
using System;

namespace org.neurul.Cortex.Port.Adapter.Admin.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                MultiHostProgram.Start(
                    new DefaultConsoleWrapper(),
                    "Neurul Cortex",
                    args,
                    new string[] { "In", "Out" },
                    new INancyBootstrapper[] { new In.Http.CustomBootstrapper(), new Out.Http.CustomBootstrapper() }
                    );
            }
            catch (Exception ex)
            {
                Console.WriteLine("The following exception occurred:");
                Console.WriteLine();
                Console.WriteLine(ex.Message);
            }
        }
    }
}
