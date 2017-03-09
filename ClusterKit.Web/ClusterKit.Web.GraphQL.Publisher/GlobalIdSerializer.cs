// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GlobalIdSerializer.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Serializes and deserializes globalId
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.GraphQL.Publisher
{
    using System.IO;
    using System.IO.Compression;
    using System.Text;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Serializes and deserializes globalId
    /// </summary>
    public static class GlobalIdSerializer
    {
        /// <summary>
        /// Packs a globalId <see cref="JObject"/> to base64
        /// </summary>
        /// <param name="globalId">The global id</param>
        /// <returns>Packed global id</returns>
        public static string PackGlobalId(this JObject globalId)
        {
            var @string = globalId.ToString(Formatting.None);
            using (var mem = new MemoryStream())
            {
                using (var zip = new GZipStream(mem, CompressionLevel.Optimal, true))
                {
                    var buffer = Encoding.UTF8.GetBytes(@string);
                    zip.Write(buffer, 0, buffer.Length);
                    zip.Close();

                    var zipped = mem.ToArray();
                    return System.Convert.ToBase64String(zipped); 
                }
            }
        }

        /// <summary>
        /// Unpacks a globalId <see cref="JObject"/> to base64
        /// </summary>
        /// <param name="packed">The packed global id</param>
        /// <returns>Unpacked global id as string</returns>
        public static string UnpackGlobalId(this string packed)
        {
            try
            {
                var zipped = System.Convert.FromBase64String(packed);
                using (var memIn = new MemoryStream())
                {
                    memIn.Write(zipped, 0, zipped.Length);
                    memIn.Position = 0;

                    const int BufferSize = 4096;
                    byte[] buffer = new byte[BufferSize];
                    using (var memOut = new MemoryStream())
                    using (var zip = new GZipStream(memIn, CompressionMode.Decompress))
                    {
                        int count;
                        do
                        {
                            count = zip.Read(buffer, 0, BufferSize);
                            if (count > 0)
                            {
                                memOut.Write(buffer, 0, count);
                            }
                        }
                        while (count > 0);
                        return Encoding.UTF8.GetString(memOut.ToArray());
                    }
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
