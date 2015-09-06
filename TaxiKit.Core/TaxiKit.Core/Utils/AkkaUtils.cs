using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxiKit.Core.Utils
{
    using Akka.Actor;

    using JetBrains.Annotations;

    /// <summary>
    /// Just some helpers
    /// </summary>
    [UsedImplicitly]
    public static class AkkaUtils
    {
        [UsedImplicitly]
        public static T DeserializeFromAkka<T>(this byte[] serializedData, ActorSystem system)
        {
            return (T)system.Serialization.FindSerializerForType(typeof(T)).FromBinary(serializedData, typeof(T));
        }

        [UsedImplicitly]
        public static T DeserializeFromAkkaString<T>(this string serializedData, ActorSystem system)
        {
            return Convert.FromBase64String(serializedData).DeserializeFromAkka<T>(system);
        }

        [UsedImplicitly]
        public static byte[] SerializeToAkka(this object objToSerialize, ActorSystem system)
        {
            return system.Serialization.FindSerializerFor(objToSerialize).ToBinary(objToSerialize);
        }

        [UsedImplicitly]
        public static string SerializeToAkkaString(this object objToSerialize, ActorSystem system)
        {
            return Convert.ToBase64String(objToSerialize.SerializeToAkka(system));
        }
    }
}