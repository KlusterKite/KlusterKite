// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ActorSystemUtils.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Utilities to work with <seealso cref="ActorSystem" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Core
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Akka.Actor;
    using Akka.DI.Core;
    using Castle.Windsor;
    using Castle.Windsor.Installer;

    /// <summary>
    /// Utilities to work with <seealso cref="ActorSystem"/>
    /// </summary>
    public static class ActorSystemUtils
    {
        /// <summary>
        /// Scans main application directory for libraries and windsor installers in them and installs them
        /// </summary>
        /// <param name="container"> 
        /// Current windsor container
        /// </param>
        /// <param name="installAssemblies">
        /// A value indicating whether this should scan current directory and load assemblies
        /// </param>
        public static void RegisterWindsorInstallers(this IWindsorContainer container, bool installAssemblies = true)
        {
            if (installAssemblies)
            {
                var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (!string.IsNullOrWhiteSpace(dir))
                {
                    var files = Directory.GetFiles(dir, "*.dll");
                    ResolveEventHandler handler = (sender, args) => Assembly.ReflectionOnlyLoad(args.Name);
                    AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += handler;

                    string[] stopList = new[] { "System.Runtime.InteropServices.RuntimeInformation" };
                    
                    foreach (var file in files)
                    {
                        try
                        {
                            var assemblyName = AssemblyName.GetAssemblyName(file);
                            if (stopList.Contains(assemblyName.FullName))
                            {
                                continue;
                            }

                            if (assemblyName != null)
                            {
                                // workaround of https://github.com/dotnet/corefx/pull/18307 and https://github.com/dotnet/corefx/issues/15112
                                if (Type.GetType("Mono.Runtime") != null)
                                {
                                    var assembly = Assembly.ReflectionOnlyLoadFrom(file);
                                    var preferInbox = assembly.GetCustomAttributes(typeof(AssemblyMetadataAttribute))
                                        .OfType<AssemblyMetadataAttribute>()
                                        .FirstOrDefault(a => a.Key == "PreferInbox");
                                    if (preferInbox?.Value.ToLower() == "true")
                                    {
                                        continue;
                                    }
                                }
                                
                                Assembly.LoadFrom(file);
                            }
                        }
                        catch 
                        {
                            // ignore
                        }
                    }

                    AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve -= handler;
                }
            }

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic))
            {
                try
                {
                    container.Install(FromAssembly.Instance(assembly));
                }
                catch (Exception)
                {
                    // ignore
                }
            }
        }

        /// <summary>
        /// Parses hocon configuration of actor system and starts <seealso cref="NameSpaceActor"/> that was defined in it
        /// </summary>
        /// <param name="sys">The actor system</param>
        public static void StartNameSpaceActorsFromConfiguration(this ActorSystem sys)
        {
            var config = sys.Settings.Config.GetConfig("akka.actor.deployment");
            if (config == null)
            {
                return;
            }

            foreach (var pair in config.AsEnumerable())
            {
                var key = pair.Key;
                var path = key.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (path.Length != 1)
                {
                    continue;
                }

                var isNameSpace = config.GetConfig(key).GetBoolean("IsNameSpace");
                if (isNameSpace)
                {
                    sys.ActorOf(sys.DI().Props(typeof(NameSpaceActor)), path[0]);
                    sys.Log.Info(
                        "{Type}: starting namespace {NameSpaceName}",
                        typeof(ActorSystemUtils).Name,
                        path[0]);
                }
            }
        }
    }
}