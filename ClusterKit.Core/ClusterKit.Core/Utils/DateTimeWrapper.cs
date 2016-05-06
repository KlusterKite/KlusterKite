// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DateTimeWrapper.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   DateTime mock. Should be used everywhere in apllication, so it could be easely tested
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Core.Utils
{
    using System;

    using JetBrains.Annotations;

    /// <summary>
    /// DateTime mock. Should be used everywhere in application, so it could be easily tested
    /// </summary>
    public static class DateTimeWrapper
    {
        /// <summary>
        /// Current time shift (in time travel jumps)
        /// </summary>
        private static TimeSpan shift = TimeSpan.Zero;

        /// <summary>
        /// Time travel just happened
        /// </summary>
        public static event Action<TimeSpan> ShiftUpdated;

        /// <summary>
        /// Gets or sets actual time getter override for tests
        /// </summary>
        public static Func<DateTimeOffset> NowGetter { get; set; }

        /// <summary>
        /// Forwarder to <seealso cref="DateTimeOffset.Now"/>
        /// </summary>
        public static DateTimeOffset Now
        {
            get
            {
#if DEBUG
                return NowGetter?.Invoke() ?? DateTimeOffset.Now + Shift;
#else
                return NowGetter?.Invoke() ?? DateTimeOffset.Now;
#endif
            }
        }

        /// <summary>
        /// Gets or sets current time shift (in time travel jumps)
        /// </summary>
        [UsedImplicitly]
        public static TimeSpan Shift
        {
            get
            {
                return shift;
            }
            set
            {
                shift = value;
                try
                {
                    ShiftUpdated?.Invoke(value);
                }
                catch (NullReferenceException)
                {
                }
            }
        }

        /// <summary>
        /// Check, that shif is enabled
        /// </summary>
        /// <returns>Shift is enabled</returns>
        public static bool IsShiftingEnabled()
        {
            return Shift != TimeSpan.Zero;
        }
    }
}