﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ClusterKit.Web.Tests.GraphQL {
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
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("ClusterKit.Web.Tests.GraphQL.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to query IntrospectionQuery {
        ///    __schema {
        ///      queryType { name }
        ///      mutationType { name }
        ///      subscriptionType { name }
        ///      types {
        ///        ...FullType
        ///      }
        ///      directives {
        ///        name
        ///        description
        ///        locations
        ///        args {
        ///          ...InputValue
        ///        }
        ///      }
        ///    }
        ///  }
        ///
        ///  fragment FullType on __Type {
        ///    kind
        ///    name
        ///    description
        ///    fields(includeDeprecated: true) {
        ///      name
        ///      description
        ///      args {
        ///        ...InputValue
        ///      }
        /// [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string IntrospectionQuery {
            get {
                return ResourceManager.GetString("IntrospectionQuery", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {
        ///  &quot;data&quot;: {
        ///    &quot;__schema&quot;: {
        ///      &quot;queryType&quot;: {
        ///        &quot;name&quot;: &quot;Query&quot;
        ///      },
        ///      &quot;mutationType&quot;: null,
        ///      &quot;subscriptionType&quot;: null,
        ///      &quot;types&quot;: [
        ///        {
        ///          &quot;kind&quot;: &quot;SCALAR&quot;,
        ///          &quot;name&quot;: &quot;String&quot;,
        ///          &quot;description&quot;: null,
        ///          &quot;fields&quot;: null,
        ///          &quot;inputFields&quot;: null,
        ///          &quot;interfaces&quot;: null,
        ///          &quot;enumValues&quot;: null,
        ///          &quot;possibleTypes&quot;: null
        ///        },
        ///        {
        ///          &quot;kind&quot;: &quot;SCALAR&quot;,
        ///          &quot;name&quot;: &quot;Boolean&quot;,
        ///          &quot;description&quot;:  [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string SchemaDescriptionTestSnapshot {
            get {
                return ResourceManager.GetString("SchemaDescriptionTestSnapshot", resourceCulture);
            }
        }
    }
}
