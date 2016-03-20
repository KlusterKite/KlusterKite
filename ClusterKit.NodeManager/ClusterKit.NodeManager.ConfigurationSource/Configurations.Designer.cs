﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.ConfigurationSource {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Configurations {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Configurations() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("ClusterKit.NodeManager.ConfigurationSource.Configurations", typeof(Configurations).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {
        ///  ClusterKit {
        ///
        ///    NodeManager.ConfigurationDatabaseConnectionString = &quot;User ID=postgres;Host=configDb;Port=5432;Pooling=true&quot;
        ///
        ///    Web {
        ///
        ///      Swagger.Publish {
        ///          publishDocPath = &quot;&quot;clusterkit/manager/swagger/doc&quot;&quot;
        ///          publishUiPath = &quot;&quot;clusterkit/manager/ui&quot;&quot;
        ///      }
        ///
        ///      Services {
        ///        ClusterKit/Web/Swagger { // ServiceName is just unique service identification, used in order to handle stacked config properly. It is used just localy on node
        ///          Port = 8080 //  [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string ClusterManager {
            get {
                return ResourceManager.GetString("ClusterManager", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {
        ///  ClusterKit {
        ///
        ///    NodeManager.ConfigurationDatabaseConnectionString = &quot;User ID=postgres;Host=configDb;Port=5432;Pooling=true&quot;
        ///
        ///    Web {
        ///
        ///      Swagger.Publish {
        ///          publishDocPath = &quot;&quot;clusterkit/manager/swagger/doc&quot;&quot;
        ///          publishUiPath = &quot;&quot;clusterkit/manager/ui&quot;&quot;
        ///      }
        ///
        ///      Services {
        ///        ClusterKit/Web/Swagger { // ServiceName is just unique service identification, used in order to handle stacked config properly. It is used just localy on node
        ///          Port = 8080 //  [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Empty {
            get {
                return ResourceManager.GetString("Empty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {
        ///  ClusterKit {
        ///    
        ///      Nginx {
        ///        PathToConfig = &quot;/etc/nginx/sites-enabled/clusterkit.config&quot;
        ///        ReloadCommand {
        ///          Command = /etc/init.d/nginx
        ///          Arguments = reload
        ///        } 
        ///        Configuration {
        ///          default {
        ///            &quot;location /&quot; { // you can define static content. This part will be just inserted into nginx config
        ///              root = /opt/web/monitoring/
        ///              index = index.html
        ///            }
        ///
        ///            &quot;location /signalr&quot; {
        ///           [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Seed {
            get {
                return ResourceManager.GetString("Seed", resourceCulture);
            }
        }
    }
}
