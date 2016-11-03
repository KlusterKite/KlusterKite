// --------------------------------------------------------------------------------------------------------------------
// <copyright file="INotificationEnveloper.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Factory class that envelopes <see cref="ParcelNotification" /> into envelopes for proper delivery
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.LargeObjects
{
    using ClusterKit.LargeObjects.Client;

    /// <summary>
    /// Factory class that envelopes <see cref="ParcelNotification"/> for proper delivery
    /// </summary>
    public interface INotificationEnveloper
    {
        /// <summary>
        /// Envelopes <see cref="ParcelNotification"/> for proper delivery
        /// </summary>
        /// <param name="parcel">The original parcel</param>
        /// <param name="notification">The parcel notification</param>
        /// <returns>The enveloped notification or null if notification does not require enveloping</returns>
        /// <remarks>
        /// Always return null if no enveloping required. This will allow other envelopers process this notification
        /// </remarks>
        object Envelope(Parcel parcel, ParcelNotification notification);
    }
}
