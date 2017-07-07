// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProcessHelper.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   A bundle of utilities for stream communication
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Launcher.Utils
{
    using System.IO;

    using Newtonsoft.Json;

    /// <summary>
    /// A bundle of utilities for stream communication
    /// </summary>
    public static class ProcessHelper
    {
        /// <summary>
        /// A string indicating end of serialized message
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public const string EOF = "###EOF###";

        /// <summary>
        /// The JSON settings
        /// </summary>
        private static readonly JsonSerializerSettings JsonSettings =
            new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };

        /// <summary>
        /// Sends the object to stream
        /// </summary>
        /// <param name="stream">The stream to send</param>
        /// <param name="message">The object</param>
        public static void Send(this TextWriter stream, object message)
        {
            var serializeObject = JsonConvert.SerializeObject(message, JsonSettings);
            stream.WriteLine(serializeObject);
            stream.WriteLine(EOF);
            stream.Flush();
        }

        /// <summary>
        /// Reads an object from stream
        /// </summary>
        /// <param name="stream">
        /// The stream to read
        /// </param>
        /// <returns>
        /// The received object
        /// </returns>
        public static object Receive(this TextReader stream)
        {
            var input = string.Empty;
            string line;
            while (((line = stream.ReadLine()) ?? EOF) != EOF)
            {
                input += line;
            }

            return JsonConvert.DeserializeObject(input, JsonSettings);
        }
    }
}
