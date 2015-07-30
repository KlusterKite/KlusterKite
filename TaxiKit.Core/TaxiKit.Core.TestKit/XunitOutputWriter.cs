namespace TaxiKit.Core.TestKit
{
    using System.IO;
    using System.Text;

    using Xunit.Abstractions;

    /// <summary>
    /// TextWriter to write logs to Xunit output
    /// </summary>
    public class XunitOutputWriter : TextWriter
    {
        private readonly ITestOutputHelper output;

        private readonly StringBuilder line;

        public XunitOutputWriter(ITestOutputHelper output)
        {
            this.output = output;
            this.line = new StringBuilder();
        }

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

        public override void WriteLine()
        {
            this.output.WriteLine(this.line.ToString());
            this.line.Clear();
        }

        public override Encoding Encoding => Encoding.UTF8;
    }
}