// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestLog.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The abstract log record
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.API.Tests.Mock
{
    using JetBrains.Annotations;

    using KlusterKite.API.Attributes;

    /// <summary>
    /// The abstract log record
    /// </summary>
    public abstract class TestLog
    {
        /// <summary>
        /// Gets or sets the log id
        /// </summary>
        [UsedImplicitly]
        [DeclareField(IsKey = true)]
        public int Id { get; set; }
    }

    /// <summary>
    /// First log implementation
    /// </summary>
    [ApiDescription(Name = "TestLogFirst")]
    // ReSharper disable once StyleCop.SA1402
    public class TestLogFirst : TestLog
    {
        /// <summary>
        /// Gets or sets the log message
        /// </summary>
        [UsedImplicitly]
        [DeclareField]
        public string FirstMessage { get; set; }
    }

    /// <summary>
    /// First log implementation
    /// </summary>
    [ApiDescription(Name = "TestLogSecond")]
    // ReSharper disable once StyleCop.SA1402
    public class TestLogSecond : TestLog
    {
        /// <summary>
        /// Gets or sets the log message
        /// </summary>
        [UsedImplicitly]
        [DeclareField]
        public string SecondMessage { get; set; }
    }
}
