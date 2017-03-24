// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MergedObjectType.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The merged api type description
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.GraphQL.Publisher.Internals
{
    using System.Collections.Generic;
    using System.Linq;

    using ClusterKit.API.Client;
    using ClusterKit.API.Client.Attributes.Authorization;
    using ClusterKit.Security.Client;
    using ClusterKit.Web.GraphQL.Publisher.GraphTypes;

    using global::GraphQL.Language.AST;
    using global::GraphQL.Resolvers;
    using global::GraphQL.Types;

    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The merged api type description
    /// </summary>
    internal class MergedObjectType : MergedFieldedType
    {
        /// <summary>
        /// Gets the name of the internal pre-calculated field to store globalId
        /// </summary>
        internal const string GlobalIdPropertyName = "__newGlobalId";

        /// <summary>
        /// Gets the name of the internal pre-calculated field to store request data for the current object
        /// </summary>
        internal const string RequestPropertyName = "_localRequest";

        /// <summary>
        /// the list of providers
        /// </summary>
        private readonly List<FieldProvider> providers = new List<FieldProvider>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MergedObjectType"/> class.
        /// </summary>
        /// <param name="originalTypeName">
        /// The original type name.
        /// </param>
        public MergedObjectType(string originalTypeName)
            : base(originalTypeName)
        {
            this.Category = EnCategory.SingleApiType;
        }

        /// <summary>
        /// The field type category
        /// </summary>
        public enum EnCategory
        {
            /// <summary>
            /// This is object provided by some single api
            /// </summary>
            SingleApiType,

            /// <summary>
            /// This is object that is combined from multiple API providers (some fields provided by one API, some from other)
            /// </summary>
            MultipleApiType
        }

        /// <summary>
        /// Gets or sets the field category
        /// </summary>
        public EnCategory Category { get; set; }

        /// <summary>
        /// Gets combined name from all provider
        /// </summary>
        public override string ComplexTypeName
        {
            get
            {
                if (this.Providers.Any())
                {
                    var providersNames =
                        this.Providers.Select(
                                p => $"{EscapeName(p.Provider.Description.ApiName)}_{EscapeName(p.FieldType.TypeName)}")
                            .Distinct()
                            .OrderBy(s => s)
                            .ToArray();

                    return string.Join("_", providersNames);
                }

                return EscapeName(this.OriginalTypeName);
            }
        }

        /// <inheritdoc />
        public override string Description
            =>
                this.Providers.Any()
                    ? string.Join(
                        "\n",
                        this.Providers.Select(p => p.FieldType.Description).Distinct().OrderBy(s => s).ToArray())
                    : null;

        /// <summary>
        /// Gets or sets the list of providers
        /// </summary>
        public IEnumerable<FieldProvider> Providers => this.providers;

        /// <summary>
        /// Gets the globalId from source data and object id value
        /// </summary>
        /// <param name="source">The source data</param>
        /// <param name="id">The id value</param>
        /// <returns>The global id</returns>
        public static JArray GetGlobalId(JObject source, JToken id)
        {
            var parent = source.Parent;
            JArray parentGlobalId = null;
            var selfRequest = source.Property(RequestPropertyName)?.Value as JObject;
            while (parent != null)
            {
                parentGlobalId = (parent as JObject)?.Property(GlobalIdPropertyName)?.Value as JArray;
                if (selfRequest == null)
                {
                    selfRequest = (parent as JObject)?.Property(RequestPropertyName)?.Value as JObject;
                }

                if (parentGlobalId != null)
                {
                    break;
                }

                parent = parent.Parent;
            }

            if (source.Property(GlobalIdPropertyName) == null && selfRequest != null)
            {
                var globalId = parentGlobalId != null ? new JArray(parentGlobalId) : new JArray();
                var selfPart = (JObject)selfRequest.DeepClone();
                if (id != null)
                {
                    selfPart.Add("id", id);
                }

                globalId.Add(selfPart);
                return globalId;
            }

            return null;
        }

        /// <summary>
        /// Adds a provider to the provider list
        /// </summary>
        /// <param name="provider">The provider</param>
        public void AddProvider(FieldProvider provider)
        {
            this.providers.Add(provider);
        }

        /// <summary>
        /// Adds the list of providers to the provider list
        /// </summary>
        /// <param name="newProviders">The list of providers</param>
        public void AddProviders(IEnumerable<FieldProvider> newProviders)
        {
            this.providers.AddRange(newProviders);
        }

        /// <summary>
        /// Makes a duplicate of the current object
        /// </summary>
        /// <returns>The object duplicate</returns>
        public virtual MergedObjectType Clone()
        {
            var mergedObjectType = new MergedObjectType(this.OriginalTypeName);
            this.FillWithMyFields(mergedObjectType);
            return mergedObjectType;
        }

        /// <summary>
        /// Gather request parameters for the specified api provider
        /// </summary>
        /// <param name="provider">
        /// The api provider
        /// </param>
        /// <param name="contextFieldAst">
        /// The request context
        /// </param>
        /// <param name="context">
        /// The resolve context.
        /// </param>
        /// <returns>
        /// The list of api requests
        /// </returns>
        public IEnumerable<ApiRequest> GatherMultipleApiRequest(
            ApiProvider provider,
            Field contextFieldAst,
            ResolveFieldContext context)
        {
            if (this.KeyField != null && this.KeyField.Providers.Contains(provider))
            {
                yield return new ApiRequest { Alias = "__id", FieldName = this.KeyField.FieldName };
            }

            // todo: process the __id request
            var requestedFields =
                GetRequestedFields(contextFieldAst.SelectionSet, context, this.ComplexTypeName).ToList();
            var usedFields =
                requestedFields.Join(
                    this.Fields.Where(f => f.Value.Providers.Any(fp => fp == provider)),
                    s => s.Name,
                    fp => fp.Key,
                    (s, fp) => new { Ast = s, Field = fp.Value }).ToList();

            foreach (var usedField in usedFields)
            {
                var apiField = usedField.Field.OriginalFields[provider.Description.ApiName];
                if (apiField != null
                    && !apiField.CheckAuthorization(context.UserContext as RequestContext, EnConnectionAction.Query))
                {
                    var severity = apiField.LogAccessRules.Any()
                                       ? apiField.LogAccessRules.Max(l => l.Severity)
                                       : EnSeverity.Trivial;

                    SecurityLog.CreateRecord(
                        SecurityLog.EnType.OperationDenied,
                        severity,
                        context.UserContext as RequestContext,
                        "Unauthorized call to {ApiPath}",
                        $"{contextFieldAst.Name}.{apiField.Name}");

                    continue;
                }

                var request = new ApiRequest
                                  {
                                      Arguments = usedField.Ast.Arguments.ToJson(context),
                                      Alias = usedField.Ast.Alias ?? usedField.Ast.Name,
                                      FieldName = usedField.Field.FieldName
                                  };
                var endType = usedField.Field.Type as MergedObjectType;

                request.Fields = endType?.Category == EnCategory.MultipleApiType
                                     ? endType.GatherMultipleApiRequest(provider, usedField.Ast, context).ToList()
                                     : usedField.Field.Type.GatherSingleApiRequest(usedField.Ast, context).ToList();

                yield return request;
            }
        }

        /// <inheritdoc />
        public override IGraphType GenerateGraphType(NodeInterface nodeInterface)
        {
            var graphType = (VirtualGraphType)base.GenerateGraphType(nodeInterface);
            var idField = graphType.Fields.FirstOrDefault(f => f.Name == "id");
            if (idField != null)
            {
                idField.Name = "__id";
            }

            graphType.AddField(
                new FieldType { Name = "id", ResolvedType = new IdGraphType(), Resolver = new GlobalIdResolver() });

            graphType.AddResolvedInterface(nodeInterface);
            nodeInterface.AddImplementedType(this.ComplexTypeName, graphType);

            return graphType;
        }

        /// <inheritdoc />
        public override object Resolve(ResolveFieldContext context)
        {
            var resolve = base.Resolve(context) as JObject;
            if (resolve == null)
            {
                return null;
            }

            return this.ResolveData(context, resolve);
        }

        /// <summary>
        /// Resolves parent data
        /// </summary>
        /// <param name="context">The request context</param>
        /// <param name="source">The parent data</param>
        /// <returns>The resolved data</returns>
        public virtual JObject ResolveData(ResolveFieldContext context, JObject source)
        {
            // setting request data for child elements
            foreach (var field in GetRequestedFields(context.FieldAst.SelectionSet, context, this.ComplexTypeName))
            {
                MergedField localField;
                if (!this.Fields.TryGetValue(field.Name, out localField))
                {
                    continue;
                }

                var propertyName = field.Alias ?? field.Name;
                var property = source.Property(propertyName)?.Value as JObject;
                if (property == null)
                {
                    if ((localField.Type as MergedObjectType)?.Category == EnCategory.MultipleApiType)
                    {
                        source.Add(propertyName, new JObject());
                        property = (JObject)source.Property(propertyName).Value;
                    }
                    else
                    {
                        continue;
                    }
                }
                
                if (property.Property(RequestPropertyName) != null)
                {
                    continue;
                }

                var localRequest = new JObject { { "f", localField.FieldName } };
                if (field.Arguments != null && field.Arguments.Any())
                {
                    var args =
                        field.Arguments.Where(
                                a =>
                                    localField.Arguments.ContainsKey(a.Name)
                                    && !localField.Arguments[a.Name].Flags.HasFlag(EnFieldFlags.IsTypeArgument))
                            .OrderBy(a => a.Name)
                            .ToList();
                    if (args.Count > 0)
                    {
                        localRequest.Add("a", args.ToJson(context));
                    }
                }

                property.Add(RequestPropertyName, localRequest);
            }

            // generating self globalId data
            var globalId = GetGlobalId(source, source.Property("__id")?.Value);
            if (globalId != null)
            {
                source.Add(GlobalIdPropertyName, globalId);
            }

            return source;
        }

        /// <summary>
        /// Fills the empty object with current objects fields
        /// </summary>
        /// <param name="shell">The empty object to fill</param>
        protected virtual void FillWithMyFields(MergedObjectType shell)
        {
            shell.AddProviders(this.providers);
            shell.Fields = this.Fields.ToDictionary(p => p.Key, p => p.Value.Clone());
            shell.Category = this.Category;
        }

        /// <summary>
        /// Resolves value for global id
        /// </summary>
        private class GlobalIdResolver : IFieldResolver
        {
            /// <inheritdoc />
            public object Resolve(ResolveFieldContext context)
            {
                var id = (context.Source as JObject)?.Property("__newGlobalId")?.Value;
                return id?.PackGlobalId();
            }
        }
    }
}