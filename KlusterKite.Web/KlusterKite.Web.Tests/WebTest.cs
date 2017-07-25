// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WebTest.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Base class for all web tests
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Web.Tests
{
    using KlusterKite.Core.TestKit;

    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Base class for all web tests
    /// </summary>
    /// <typeparam name="TConfigurator">
    /// Class, that describes test configuration
    /// </typeparam>
    [Collection("KlusterKite.Web.Tests")]
    public abstract class WebTest<TConfigurator> : BaseActorTest<TConfigurator> where TConfigurator : TestConfigurator, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebTest{TConfigurator}"/> class.
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        protected WebTest(ITestOutputHelper output) : base(output)
        {
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            Startup.Reset();
            base.Dispose(disposing);
        }
    }
}
