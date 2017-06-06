// --------------------------------------------------------------------------------------------------------------------
// <copyright file="XunitOutputWriter.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   TextWriter to write logs to Xunit output
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Core.TestKit
{
    using System.IO;
    using System.Text;

    using Xunit.Abstractions;

    /// <summary>
    /// TextWriter to write logs to Xunit output
    /// </summary>
    public class XunitOutputWriter : TextWriter
    {
        /// <summary>
        /// the line to write
        /// </summary>
        private readonly StringBuilder line;

        /// <summary>
        /// The xunit output
        /// </summary>
        private readonly ITestOutputHelper output;

        /// <summary>
        /// Initializes a new instance of the <see cref="XunitOutputWriter"/> class.
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        public XunitOutputWriter(ITestOutputHelper output)
        {
            this.output = output;
            this.line = new StringBuilder();
        }

        /// <inheritdoc />
        public override Encoding Encoding => Encoding.UTF8;

        /// <inheritdoc />
        public override void Write(char[] buffer)
        {
            var str = new string(buffer);
            if (!str.EndsWith(this.NewLine))
            {
                this.line.Append(str);
                return;
            }

            this.line.Append(str.Substring(0, str.Length - this.NewLine.Length));
            this.output.WriteLine(this.line.ToString());
            this.line.Clear();
        }

        /// <inheritdoc />
        public override void WriteLine()
        {
            this.output.WriteLine(this.line.ToString());
            this.line.Clear();
        }
    }
} 