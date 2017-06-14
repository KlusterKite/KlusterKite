// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigurationUtils.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   A bundle of methods to tune executable configuration file
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Launcher.Utils
{
    using System.IO;
    using System.Reflection;

#if APPDOMAIN
    using System;
    using System.Globalization;
    using System.Xml;
#endif

    /// <summary>
    /// A bundle of methods to tune executable configuration file
    /// </summary>
    public static class ConfigurationUtils
    {
        /// <summary>
        /// Fixes service configuration file to pass possible version conflicts in dependent assemblies
        /// </summary>
        /// <param name="configurationFileName">
        /// The configuration file name
        /// </param>
        public static void FixAssemblyVersions(string configurationFileName)
        {
#if APPDOMAIN
            var document = new XmlDocument();
            document.Load(configurationFileName);
            var documentElement = document.DocumentElement;
            if (documentElement == null)
            {
                Console.WriteLine($@"Configuration file {configurationFileName} is broken");
                return;
            }

            documentElement = (XmlElement)documentElement.SelectSingleNode("/configuration");
            if (documentElement == null)
            {
                Console.WriteLine($@"Configuration file {configurationFileName} is broken");
                return;
            }

            var runTimeNode = documentElement.SelectSingleNode("./runtime")
                              ?? documentElement.AppendChild(document.CreateElement("runtime"));

            var nameTable = document.NameTable;
            var namespaceManager = new XmlNamespaceManager(nameTable);
            const string Uri = "urn:schemas-microsoft-com:asm.v1";
            namespaceManager.AddNamespace("urn", Uri);

            var assemblyBindingNode = runTimeNode.SelectSingleNode("./urn:assemblyBinding", namespaceManager)
                                      ?? runTimeNode.AppendChild(document.CreateElement("assemblyBinding", Uri));

            foreach (var lib in Directory.GetFiles(Path.GetDirectoryName(configurationFileName) ?? configurationFileName, "*.dll"))
            {
                var parameters = AssemblyName.GetAssemblyName(lib);
                if (parameters == null)
                {
                    continue;
                }

                var dependentNode =
                    assemblyBindingNode?.SelectSingleNode(
                        $"./urn:dependentAssembly[./urn:assemblyIdentity/@name='{parameters.Name}']",
                        namespaceManager)
                    ?? assemblyBindingNode?.AppendChild(document.CreateElement("dependentAssembly", Uri));

                if (dependentNode == null)
                {
                    continue;
                }

                dependentNode.RemoveAll();
                var assemblyIdentityNode =
                    (XmlElement)dependentNode.AppendChild(document.CreateElement("assemblyIdentity", Uri));
                assemblyIdentityNode.SetAttribute("name", parameters.Name);
                var publicKeyToken =
                    BitConverter.ToString(parameters.GetPublicKeyToken())
                        .Replace("-", string.Empty)
                        .ToLower(CultureInfo.InvariantCulture);
                assemblyIdentityNode.SetAttribute("publicKeyToken", publicKeyToken);
                var bindingRedirectNode =
                    (XmlElement)dependentNode.AppendChild(document.CreateElement("bindingRedirect", Uri));
                bindingRedirectNode.SetAttribute("oldVersion", $"0.0.0.0-{parameters.Version}");
                bindingRedirectNode.SetAttribute("newVersion", parameters.Version.ToString());
            }

            document.Save(configurationFileName);
#endif
        }

        /// <summary>
        /// Reads texts resource
        /// </summary>
        /// <param name="assembly">The resource containing assembly</param>
        /// <param name="resourceName">The resource name</param>
        /// <returns>The resource contents</returns>
        public static string ReadTextResource(Assembly assembly, string resourceName)
        {
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    return null;
                }

                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
