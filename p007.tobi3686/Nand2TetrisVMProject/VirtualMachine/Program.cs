using System;
using System.Threading;
using VirtualMachine.UI;
using VirtualMachine.Enums;
using VirtualMachine.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using Microsoft.Extensions.Hosting;
using VirtualMachine.Converters;
using VirtualMachine.Handlers;
using VirtualMachine.Core;

namespace VirtualMachine
{
    public class Program
    {
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            BuildConfig(builder);
            var host = CreateHostConfiguration();
            var startupService = ActivatorUtilities.GetServiceOrCreateInstance<Startup>(host.Services);

            Console.Write("Enter a valid root dir containing the files : ");
            var rootDir = Console.ReadLine();

            Console.Write("Enter a valid root output dir for the result : ");
            var rootOutputDir = Console.ReadLine();

            Console.Write("Enter a valid search pattern for the files eg. *.vm : ");
            var searchOption = Console.ReadLine();

            startupService.RunTranslator(rootDir, rootOutputDir, searchOption);

            ConsoleUIManager.Beautify(Color.Default, "Exiting Translator in 5 Seconds.");
            Thread.Sleep(5000);
            EnvironmentHelper.ExitEnvironment();
        }

        private static void BuildConfig(IConfigurationBuilder builder)
        {
            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{EnvironmentHelper.GetAspNetCoreEnvironmentVariable() ?? "Production"}.json", optional: true)
                .AddEnvironmentVariables();
        }

        private static IHost CreateHostConfiguration()
        {
            return Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddScoped<AssemblyMachineCodeTranslator>();
                    services.AddScoped<CommandHandler>();
                    services.AddScoped<IMachineCodeConverter, VirtualMachineCodeConverter>();
                    services.AddScoped<FileHandler>();
                })
                .Build();
        }
    }
}
