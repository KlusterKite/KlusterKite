// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Extensions.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   A bundle of extension methods
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Data.CRUD
{
    using Akka.Actor;

    using KlusterKite.Core.Utils;

    /// <summary>
    /// A bundle of extension methods
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Sets the extra data to the message
        /// </summary>
        /// <typeparam name="T">
        /// The type of message
        /// </typeparam>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="extraData">
        /// Some extra data object
        /// </param>
        /// <param name="system">
        /// The akka system
        /// </param>
        /// <returns>
        /// This message itself for chaining purposes
        /// </returns>
        public static T SetExtraData<T>(this T message, object extraData, ActorSystem system) where T : IMessageWithExtraData
        {
            message.ExtraData = extraData.SerializeToAkka(system);
            return message;
        }

        /// <summary>
        /// Extracts some extra data from message
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="system">
        /// The actor system
        /// </param>
        /// <returns>
        /// Deserialized object
        /// </returns>
        public static object GetExtraData(this IMessageWithExtraData message, ActorSystem system)
        {
            return message.ExtraData?.DeserializeFromAkka<object>(system);
        }
    }
}
