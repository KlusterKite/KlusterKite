// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SurrogatableJObject.cs" company="KlusterKite">
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

    using JetBrains.Annotations;

    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Safely serializable JSON object
    /// </summary>
    public class SurrogatableJObject : ISurrogated
    {
        /// <summary>
        /// Gets or sets the json value
        /// </summary>
        [UsedImplicitly]
        public JObject Json { get; set; }

        /// <summary>
        /// Converts original object to wrapper
        /// </summary>
        /// <param name="obj">The original object</param>
        public static implicit operator SurrogatableJObject(JObject obj)
        {
            return new SurrogatableJObject { Json = obj };
        }

        /// <summary>
        /// Converts wrapper to the original object
        /// </summary>
        /// <param name="obj">Wrapped object</param>
        public static implicit operator JObject(SurrogatableJObject obj)
        {
            return obj?.Json;
        }

        /// <inheritdoc />
        public ISurrogate ToSurrogate(ActorSystem system)
        {
            return new SurrogateJObject { Json = this.Json?.ToString() };
        }
    }
}