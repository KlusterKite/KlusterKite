// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigurationSetObsoleteRequest.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Moves <see cref="FSMBase.State{TS,TD}"/> from <see cref="EnConfigurationState.Ready"/> to <see cref="EnConfigurationState.Obsolete"/>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Messages
{
    using Akka.Actor;

    using KlusterKite.NodeManager.Client.ORM;
    using KlusterKite.Security.Attributes;

    /// <summary>
    /// Moves <see cref="FSMBase.State{TS,TD}"/> from <see cref="EnConfigurationState.Ready"/> to <see cref="EnConfigurationState.Obsolete"/>
    /// </summary>
    /// <returns>The mutation result</returns>
    public class ConfigurationSetObsoleteRequest
    {
        /// <summary>
        /// Gets or sets the configuration id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the request context
        /// </summary>
        public RequestContext Context { get; set; }
    }
}