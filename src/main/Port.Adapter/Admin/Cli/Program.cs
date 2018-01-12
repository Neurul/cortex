using Nancy.Hosting.Self;
using org.neurul.Common.Domain.Model;
using System;

namespace org.neurul.Cortex.Port.Adapter.Admin.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine();
                Console.WriteLine("Initializing Neurul Cortex...");
                Console.WriteLine();

                AssertionConcern.AssertArgumentValid(a => a != null && a.Length == 2, args, "Must specify 'In' and 'Out' URIs.", nameof(args));
                AssertionConcern.AssertArgumentValid(a => Uri.IsWellFormedUriString(a[0], UriKind.RelativeOrAbsolute), args, "Must specify valid 'In' URI", nameof(args));
                AssertionConcern.AssertArgumentValid(a => Uri.IsWellFormedUriString(a[1], UriKind.RelativeOrAbsolute), args, "Must specify valid 'Out' URI", nameof(args));

                var host = new NancyHost(new In.Http.CustomBootstrapper(), new Uri(args[0]));
                host.Start();

                host = new NancyHost(new Out.Http.CustomBootstrapper(), new Uri(args[1]));
                host.Start();

                var response = string.Empty;

                while (response.ToUpper() != "Y")
                {
                    response = string.Empty;

                    Console.Clear();
                    Console.WriteLine($"In: {args[0]}");
                    Console.WriteLine($"Out: {args[1]}");
                    Console.WriteLine();

                    Console.WriteLine("Neurul Cortex online.");
                    Console.WriteLine();
                    Console.WriteLine("Enjoy!");
                    Console.WriteLine();

                    Console.Write("Press any key to exit...");
                    Console.ReadKey(true);
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine("Are you sure you wish to exit? (Y/N)");
                    while (response == string.Empty)
                    {
                        response = Console.ReadKey(true).KeyChar.ToString();
                        if (response.ToUpper() != "Y" && response.ToUpper() != "N")
                        {
                            response = string.Empty;
                            Console.Beep();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("The following exception occurred:");
                Console.WriteLine(ex.Message);
            }
        }
    }
}
