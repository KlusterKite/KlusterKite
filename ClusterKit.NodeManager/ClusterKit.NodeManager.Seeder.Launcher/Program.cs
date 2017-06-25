// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Service main entry point
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Seeder.Launcher
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.Versioning;
    using System.Threading;

    using Akka.Configuration;

    using ClusterKit.NodeManager.Launcher.Utils;
    using ClusterKit.NodeManager.Launcher.Utils.Exceptions;

    using JetBrains.Annotations;

    using NuGet.Frameworks;
    
    /// <summary>
    /// Service main entry point
    /// </summary>
    [UsedImplicitly]
    public class Program
    {
        /// <summary>
        /// Service main entry point
        /// </summary>
        /// <param name="args">
        /// Startup parameters
        /// </param>
        public static void Main(string[] args)
        {
            var configuration = ReadConfiguration(args);
            if (configuration == null)
            {
                return;
            }

            foreach (var seederConfiguration in configuration.Configurations)
            {
                RunSeederConfiguration(configuration, seederConfiguration);
            }
        }

        /// <summary>
        /// Runs seeder configuration
        /// </summary>
        /// <param name="configuration">The global configuration</param>
        /// <param name="seederConfiguration">The seeder configuration</param>
        private static void RunSeederConfiguration(Configuration configuration, SeederConfiguration seederConfiguration)
        {
            var executionDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(executionDir);

            try
            {
                var currentFramework = NuGetFramework.ParseFrameworkName(
                    configuration.ExecutionFramework.FullName,
                    new DefaultFrameworkNameProvider());

                while (true)
                {
                    try
                    {
                        var requiredPackages =
                            seederConfiguration.RequiredPackages.SearchLatestPackagesWithDependencies(
                                currentFramework,
                                configuration.Nuget);
                        Console.WriteLine("Packages list created");
                        //requiredPackages.Select(p => p.Identity)
                            //.Install(runtime, currentFramework, configuration.Nuget, executionDir);
                        Console.WriteLine("Packages installed");
                    }
                    catch (PackageNotFoundException packageNotFoundException)
                    {
                        Console.WriteLine($"Package {packageNotFoundException.Message} was not found");
                        Thread.Sleep(configuration.NugetCheckPeriod);
                        continue;
                    }
                    catch (AggregateException exception)
                    {
                        var e = exception.InnerExceptions?.FirstOrDefault() ?? exception;
                        Console.WriteLine(e.Message);
                        Console.WriteLine(e.StackTrace);
                        Thread.Sleep(configuration.NugetCheckPeriod);
                        continue;
                    }
                    catch (Exception exception)
                   {
                       Console.WriteLine(exception.Message);
                       Console.WriteLine(exception.StackTrace);
                        Thread.Sleep(configuration.NugetCheckPeriod);
                       continue;
                   }

                    try
                    {
                        File.Copy(configuration.ConfigFile, Path.Combine(executionDir, "seeder.hocon"), true);
                        
                        // todo: move to CreateServiceAsync
                        // ConfigurationUtils.FixAssemblyVersions(Path.Combine(executionDir, "ClusterKit.NodeManager.Seeder.exe.config"));
                        Console.WriteLine($"Seeder prepared in {executionDir}");
                        var process = new Process
                                          {
                                              StartInfo =
                                                  {
                                                      UseShellExecute = false,
                                                      WorkingDirectory = executionDir,
                                                      FileName = Path.Combine(
                                                          executionDir,
                                                          "ClusterKit.NodeManager.Seeder.exe"),
                                                      Arguments = seederConfiguration.Name
                                                  }
                                          };
                        process.Start();
                        process.WaitForExit();
                        process.Dispose();
                        Console.WriteLine("Seeder stopped");
                        break;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        Console.WriteLine(e.StackTrace);
                        Console.ReadLine();
                        break;
                    }
                }
            }
            finally
            {
                try
                {
                    Directory.Delete(executionDir, true);
                }
                catch
                {
                    Console.WriteLine("Failed to remove execution directory");
                }
            }
        }

        /// <summary>
        /// Reads the configuration
        /// </summary>
        /// <param name="args">The program command line arguments</param>
        /// <returns>The configuration</returns>
        private static Configuration ReadConfiguration(string[] args)
        {
            var configuration = new Configuration();
            configuration.ConfigFile = args != null && args.Length > 0 ? args[0] : "seeder.hocon";
            if (configuration.ConfigFile == null || !File.Exists(configuration.ConfigFile))
            {
                Console.WriteLine("Configuration file was not found");
                return null;
            }

            configuration.Config = ConfigurationFactory.ParseString(File.ReadAllText(configuration.ConfigFile));
            configuration.Nuget = configuration.Config.GetString("Nuget");
            if (string.IsNullOrWhiteSpace(configuration.Nuget))
            {
                Console.WriteLine("Nuget is not properly defined");
                return null;
            }

            configuration.NugetCheckPeriod =
                configuration.Config.GetTimeSpan("NugetCheckPeriod", TimeSpan.FromMinutes(1));
            configuration.ExecutionFramework = new FrameworkName(configuration.Config.GetString("ExecutionFramework"));

            configuration.Configurations = new List<SeederConfiguration>();
            foreach (var subConfig in configuration.Config.GetStringList("SeederConfigurations"))
            {
                var seederConfig = configuration.Config.GetConfig(subConfig);
                if (seederConfig == null)
                {
                    Console.WriteLine($"SeederConfiguration {subConfig} was not found in the config");
                    return null;
                }

                var seederConfiguration = new SeederConfiguration(subConfig, seederConfig);
                if (seederConfiguration.RequiredPackages == null || seederConfiguration.RequiredPackages.Count == 0)
                {
                    Console.WriteLine($"SeederConfiguration {subConfig} misses the RequiredPackages property");
                    return null;
                }

                if (seederConfiguration.Seeders == null || seederConfiguration.Seeders.Count == 0)
                {
                    Console.WriteLine($"SeederConfiguration {subConfig} misses the Seeders property");
                    return null;
                }

                configuration.Configurations.Add(seederConfiguration);
            }

            if (configuration.Configurations.Count == 0)
            {
                Console.WriteLine("SeederConfiguration is empty");
                return null;
            }

            return configuration;
        }
    }
}