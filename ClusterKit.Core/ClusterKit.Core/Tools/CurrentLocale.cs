namespace ClusterKit.Core.Tools
{
    using System;
    using System.Globalization;
    using System.Threading;

    using JetBrains.Annotations;

    /// <summary>
    /// Helper class to set current thread culture. Will restore original culture on dispose.
    /// </summary>
    public class CurrentLocale : IDisposable
    {
        private readonly Thread localThread;
        private readonly CultureInfo originalCulture;

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentLocale"/> class.
        /// </summary>
        /// <param name="locale">Locale code to set</param>
        public CurrentLocale([NotNull] string locale)
        {
            this.localThread = Thread.CurrentThread;
            this.originalCulture = this.localThread.CurrentUICulture;

            try
            {
                this.localThread.CurrentUICulture = CultureInfo.GetCultureInfo(locale);
            }
            catch (CultureNotFoundException)
            {
                this.localThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
            }
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            this.localThread.CurrentUICulture = this.originalCulture;
        }
    }
}