// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequestPathElement.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The request path element
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Client
{
    using JetBrains.Annotations;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The request path element
    /// </summary>
    [UsedImplicitly]
    public class RequestPathElement
    {
        /// <summary>
        /// The field argument list
        /// </summary>
        private JObject arguments;

        /// <summary>
        /// Gets or sets the field name
        /// </summary>
        [JsonProperty("f")]
        [UsedImplicitly]
        public string FieldName { get; set; }

        /// <summary>
        /// Gets or sets the field argument list
        /// </summary>
        [JsonProperty("a")]
        [UsedImplicitly]
        public JObject Arguments
        {
            get
            {
                return this.arguments;
            }

            set
            {
                this.arguments = value != null && value.HasValues ? value : null;
            }
        }

        /// <summary>
        /// Converts path element to the api request
        /// </summary>
        /// <returns>The api request</returns>
        public ApiRequest ToApiRequest()
        {
            return new ApiRequest
                       {
                           FieldName = this.FieldName,
                           Arguments = this.Arguments
                       };
        }
    }
}