using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DependentAssemblyVersionCorrector
{
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Xml;

    /// <summary>
    /// Small kludge to correct config file of a programm to confirm usage of current dependent library versions
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            if (args == null || args.Length != 2)
            {
                Console.WriteLine($"Usage: {Path.GetFileName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName)} [AppDomain direcotry] [Configuration file]");
                return;
            }

            var dirName = args[0];
            if (!Directory.Exists(dirName))
            {
                Console.WriteLine($"{dirName} is not a valid directory");
                return;
            }

            var configName = args[1];
            if (!File.Exists(configName))
            {
                Console.WriteLine($"Could not find app configuration file {configName}");
                return;
            }

            XmlDocument confDoc = new XmlDocument();
            confDoc.Load(configName);
            var documentElement = confDoc.DocumentElement;
            if (documentElement == null)
            {
                Console.WriteLine($"Configuration file {configName} is broken");
                return;
            }

            documentElement = (XmlElement)documentElement.SelectSingleNode("/configuration");
            if (documentElement == null)
            {
                Console.WriteLine($"Configuration file {configName} is broken");
                return;
            }

            var runTimeNode = documentElement.SelectSingleNode("./runtime")
                              ?? documentElement.AppendChild(confDoc.CreateElement("runtime"));

            var nameTable = confDoc.NameTable;
            var namespaceManager = new XmlNamespaceManager(nameTable);
            const string uri = "urn:schemas-microsoft-com:asm.v1";
            namespaceManager.AddNamespace("urn", uri);

            var assemblyBindingNode = runTimeNode.SelectSingleNode("./urn:assemblyBinding", namespaceManager)
                                      ?? runTimeNode.AppendChild(confDoc.CreateElement("assemblyBinding", uri));

            foreach (var lib in Directory.GetFiles(dirName, "*.dll"))
            {
                var parameters = AssemblyName.GetAssemblyName(lib);
                var dependentNode =
                    assemblyBindingNode.SelectSingleNode($"./urn:dependentAssembly[./urn:assemblyIdentity/@name='{parameters.Name}']", namespaceManager)
                    ?? assemblyBindingNode.AppendChild(confDoc.CreateElement("dependentAssembly", uri));

                dependentNode.RemoveAll();
                var assemblyIdentityNode = (XmlElement)dependentNode.AppendChild(confDoc.CreateElement("assemblyIdentity", uri));
                assemblyIdentityNode.SetAttribute("name", parameters.Name);
                assemblyIdentityNode.SetAttribute("publicKeyToken", BitConverter.ToString(parameters.GetPublicKeyToken()).Replace("-", "").ToLower(CultureInfo.InvariantCulture));
                var bindingRedirectNode = (XmlElement)dependentNode.AppendChild(confDoc.CreateElement("bindingRedirect", uri));
                bindingRedirectNode.SetAttribute("oldVersion", $"0.0.0.0-{parameters.Version}");
                bindingRedirectNode.SetAttribute("newVersion", parameters.Version.ToString());

                // Console.WriteLine($"{parameters.Name} {parameters.Version} {BitConverter.ToString(parameters.GetPublicKeyToken()).Replace("-", "").ToLower(CultureInfo.InvariantCulture)}");
                Console.WriteLine($"{parameters.Name} {parameters.Version}");
            }

            confDoc.Save(configName);
        }
    }
}