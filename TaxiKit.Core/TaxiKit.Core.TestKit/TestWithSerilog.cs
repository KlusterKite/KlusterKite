// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestWithSerilog.cs" company="TaxiKit">
//   All rights reserved
// </copyright>
// <summary>
//   Base test class that pushes serilog to Xunit output
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace TaxiKit.Core.TestKit
{
    using Serilog;

    using Xunit.Abstractions;
    using static Serilog.Log;

    /// <summary>
    /// Base test class that pushes serilog to Xunit output
    /// </summary>
    public class TestWithSerilog
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestWithSerilog"/> class.
        /// </summary>
        /// <param name="output">
        /// The Xunit output.
        /// </param>
        protected TestWithSerilog(ITestOutputHelper output)
        {
            var loggerConfig = new LoggerConfiguration().WriteTo.TextWriter(new XunitOutputWriter(output));
            Logger = loggerConfig.CreateLogger();
        }
    }
}