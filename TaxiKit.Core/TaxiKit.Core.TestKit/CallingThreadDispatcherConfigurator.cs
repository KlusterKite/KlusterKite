// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CallingThreadDispatcherConfigurator.cs" company="TaxiKit">
//   All rights reserved
// </copyright>
// <summary>
//   Empty override of standard configurator
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace TaxiKit.Core.TestKit
{
    using Akka.Configuration;
    using Akka.Dispatch;

    /// <summary>
    /// Empty override of standard configurator
    /// </summary>
    public class CallingThreadDispatcherConfigurator : MessageDispatcherConfigurator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CallingThreadDispatcherConfigurator"/> class.
        /// </summary>
        /// <param name="config">
        /// The config.
        /// </param>
        /// <param name="prerequisites">
        /// The prerequisites.
        /// </param>
        public CallingThreadDispatcherConfigurator(Config config, IDispatcherPrerequisites prerequisites)
            : base(config, prerequisites)
        {
        }

        /// <summary>
        /// Returns a <see cref="M:Akka.Dispatch.MessageDispatcherConfigurator.Dispatcher"/> instance.
        ///             Whether or not this <see cref="T:Akka.Dispatch.MessageDispatcherConfigurator"/> returns a new instance
        ///             or returns a reference to an existing instance is an implementation detail of the
        ///             underlying implementation.
        /// </summary>
        /// <returns>The message dispatcher</returns>
        public override MessageDispatcher Dispatcher()
        {
            return new CallingThreadDispatcher(this);
        }
    }
}