// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SurrogateJObject.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Safely serializable JSON object
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.API.Client.Messages
{
    using Akka.Actor;
    using Akka.Util;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Safely serializable JSON object
    /// </summary>
    public class SurrogateJObject : ISurrogate
    {
        /// <summary>
        /// Gets or sets the json value
        /// </summary>
        public string Json { get; set; }

        /// <inheritdoc />
        public ISurrogated FromSurrogate(ActorSystem system)
        {
            return new SurrogatableJObject
                       {
                           Json =
                               string.IsNullOrWhiteSpace(this.Json)
                                   ? null
                                   : JsonConvert.DeserializeObject(this.Json) as JObject
                       };
        }
    }
}