// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AkkaAddressSurrogate.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Represents <see cref="Address" /> for public API
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Client.ApiSurrogates
{
    using Akka.Actor;

    using ClusterKit.API.Client;
    using ClusterKit.API.Client.Attributes;

    using JetBrains.Annotations;

    /// <summary>
    /// Represents <see cref="Address"/> for public API
    /// </summary>
    [UsedImplicitly]
    [ApiDescription(Description = "Akka system address", Name = "AkkaAddress")]
    public class AkkaAddressSurrogate
    {
        /// <summary>
        /// Gets or sets the address string representation
        /// </summary>
        [UsedImplicitly]
        [DeclareField(Description = "the address string representation")]
        public string AsString { get; set; }

        /// <summary>
        /// Gets or sets the address host
        /// </summary>
        [UsedImplicitly]
        [DeclareField(Description = "the address host")]
        public string Host { get; set; }

        /// <summary>
        /// Gets or sets the address port
        /// </summary>
        [UsedImplicitly]
        [DeclareField(Description = "the address port")]
        public int? Port { get; set; }

        /// <summary>
        /// Gets or sets the address protocol
        /// </summary>
        [UsedImplicitly]
        [DeclareField(Description = "the address protocol")]
        public string Protocol { get; set; }

        /// <summary>
        /// Gets or sets the address system name
        /// </summary>
        [UsedImplicitly]
        [DeclareField(Description = "the address system name")]
        public string System { get; set; }

        /// <summary>
        /// Converts <see cref="Address"/> to <see cref="AkkaAddressSurrogate"/>
        /// </summary>
        public class Converter : IValueConverter<AkkaAddressSurrogate>
        {
            /// <inheritdoc />
            public AkkaAddressSurrogate Convert(object source)
            {
                var address = source as Address;
                if (address == null)
                {
                    return null;
                }

                return new AkkaAddressSurrogate
                           {
                               Host = address.Host,
                               Port = address.Port,
                               System = address.System,
                               Protocol = address.Protocol,
                               AsString = address.ToString()
                           };
            }
        }
    }
}