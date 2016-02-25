// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AkkaUtils.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Just some helpers
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Core.Utils
{
    using System;

    using Akka.Actor;

    using JetBrains.Annotations;

    /// <summary>
    /// Just some helpers
    /// </summary>
    [UsedImplicitly]
    public static class AkkaUtils
    {
        /// <summary>
        /// The deserialize from akka.
        /// </summary>
        /// <param name="serializedData">
        /// The serialized data.
        /// </param>
        /// <param name="system">
        /// The system.
        /// </param>
        /// <typeparam name="T">
        /// The original object type
        /// </typeparam>
        /// <returns>
        /// The <see cref="T"/>.
        /// </returns>
        [UsedImplicitly]
        public static T DeserializeFromAkka<T>(this byte[] serializedData, ActorSystem system)
        {
            return (T)system.Serialization.FindSerializerForType(typeof(T)).FromBinary(serializedData, typeof(T));
        }

        /// <summary>
        /// The deserialize from akka string.
        /// </summary>
        /// <param name="serializedData">
        /// The serialized data.
        /// </param>
        /// <param name="system">
        /// The system.
        /// </param>
        /// <typeparam name="T">
        /// The original object type
        /// </typeparam>
        /// <returns>
        /// The <see cref="T"/>.
        /// </returns>
        [UsedImplicitly]
        public static T DeserializeFromAkkaString<T>(this string serializedData, ActorSystem system)
        {
            return Convert.FromBase64String(serializedData).DeserializeFromAkka<T>(system);
        }

        /// <summary>
        /// The serialize to akka.
        /// </summary>
        /// <param name="objToSerialize">
        /// The object to serialize.
        /// </param>
        /// <param name="system">
        /// The system.
        /// </param>
        /// <returns>
        /// The byte array
        /// </returns>
        [UsedImplicitly]
        public static byte[] SerializeToAkka(this object objToSerialize, ActorSystem system)
        {
            return system.Serialization.FindSerializerFor(objToSerialize).ToBinary(objToSerialize);
        }

        /// <summary>
        /// The serialize to akka string.
        /// </summary>
        /// <param name="objToSerialize">
        /// The object to serialize.
        /// </param>
        /// <param name="system">
        /// The system.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        [UsedImplicitly]
        public static string SerializeToAkkaString(this object objToSerialize, ActorSystem system)
        {
            return Convert.ToBase64String(objToSerialize.SerializeToAkka(system));
        }
    }
}