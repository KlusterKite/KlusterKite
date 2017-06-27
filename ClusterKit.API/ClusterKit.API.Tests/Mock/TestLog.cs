// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestLog.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The abstract log record
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Tests.Mock
{
    /// <summary>
    /// The abstract log record
    /// </summary>
    public abstract class TestLog
    {
        /// <summary>
        /// Gets or sets the log id
        /// </summary>
        public int Id { get; set; }
    }

    /// <summary>
    /// First log implementation
    /// </summary>
    // ReSharper disable once StyleCop.SA1402
    public class TestLogFirst : TestLog
    {
        /// <summary>
        /// Gets or sets the log message
        /// </summary>
        public string FirstMessage { get; set; }
    }

    /// <summary>
    /// First log implementation
    /// </summary>
    // ReSharper disable once StyleCop.SA1402
    public class TestLogSecond : TestLog
    {
        /// <summary>
        /// Gets or sets the log message
        /// </summary>
        public string SecondMessage { get; set; }
    }
}
