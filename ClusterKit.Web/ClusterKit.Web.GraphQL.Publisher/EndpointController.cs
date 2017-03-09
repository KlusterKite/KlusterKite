// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EndpointController.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   GraphQL endpoint controller
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.GraphQL.Publisher
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Http;

    using ClusterKit.Web.Authorization;

    using global::GraphQL;
    using global::GraphQL.Http;
    using global::GraphQL.Instrumentation;

    using JetBrains.Annotations;

    using Newtonsoft.Json.Linq;

    /// <summary>
    /// GraphQL endpoint controller
    /// </summary>
    [RoutePrefix("api/1.x/graphQL")]
    public class EndpointController : ApiController
    {
        /// <summary>
        /// The executer.
        /// </summary>
        private readonly IDocumentExecuter executer;

        /// <summary>
        /// The schema provider.
        /// </summary>
        private readonly SchemaProvider schemaProvider;

        /// <summary>
        /// The writer.
        /// </summary>
        private readonly IDocumentWriter writer;

        /// <summary>
        /// Initializes a new instance of the <see cref="EndpointController"/> class.
        /// </summary>
        /// <param name="schemaProvider">
        /// The schema provider.
        /// </param>
        /// <param name="executer">
        /// The executer.
        /// </param>
        /// <param name="writer">
        /// The writer.
        /// </param>
        public EndpointController(SchemaProvider schemaProvider, IDocumentExecuter executer, IDocumentWriter writer)
        {
            this.schemaProvider = schemaProvider;
            this.executer = executer;
            this.writer = writer;
        }

        /// <summary>
        /// Processes the GraphQL post request
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="query">The query data</param>
        /// <returns>GraphQL response</returns>
        [HttpPost]
        [Route]
        public async Task<HttpResponseMessage> Post(HttpRequestMessage request, JObject query)
        {
            var queryToExecute =
                (string)
                (query.Properties()
                         .FirstOrDefault(p => p.Name.Equals("query", StringComparison.InvariantCultureIgnoreCase))
                         ?.Value as JValue);

            if (string.IsNullOrWhiteSpace(queryToExecute))
            {
                return request.CreateResponse(HttpStatusCode.BadRequest);
            }

            var operationName =
                (string)
                (query.Properties()
                     .FirstOrDefault(p => p.Name.Equals("OperationName", StringComparison.InvariantCultureIgnoreCase))
                     ?.Value as JValue);

            var variablesToken =
                query.Properties()
                    .FirstOrDefault(
                        p => p.Name.Equals("variables", StringComparison.InvariantCultureIgnoreCase))?.Value;

            Inputs inputs = null;
            if (variablesToken is JObject)
            {
                inputs = variablesToken.ToString().ToInputs();
            }
            else if (variablesToken is JValue)
            {
                inputs = ((JValue)variablesToken).ToObject<string>()?.ToInputs();
            }

            var requestContext = this.GetRequestDescription();
            var schema = this.schemaProvider.CurrentSchema;
            if (schema == null)
            {
                return request.CreateResponse(HttpStatusCode.ServiceUnavailable);
            }

            var result = await this.executer.ExecuteAsync(
                             options =>
                                 {
                                     options.Schema = schema;
                                     options.Query = queryToExecute;
                                     options.OperationName = operationName;
                                     options.Inputs = inputs;
                                     options.UserContext = requestContext;

                                     // _.ComplexityConfiguration = new ComplexityConfiguration { MaxDepth = 15 };
                                     options.FieldMiddleware.Use<InstrumentFieldsMiddleware>();
                                 }).ConfigureAwait(false);

            var httpResult = result.Errors?.Count > 0 ? HttpStatusCode.BadRequest : HttpStatusCode.OK;

            var json = this.writer.Write(result);

            var response = request.CreateResponse(httpResult);
            response.Content = new StringContent(json, Encoding.UTF8, "application/json");

            return response;
        }

        /// <summary>
        /// The GraphQL http request body description 
        /// </summary>
        public class QueryRequest
        {
            /// <summary>
            /// Gets or sets the operation name
            /// </summary>
            [UsedImplicitly]
            public string OperationName { get; set; }

            /// <summary>
            /// Gets or sets the query
            /// </summary>
            [UsedImplicitly]
            public string Query { get; set; }

            /// <summary>
            /// Gets or sets the list of defined variables
            /// </summary>
            [UsedImplicitly]
            public string Variables { get; set; }
        }
    }
}