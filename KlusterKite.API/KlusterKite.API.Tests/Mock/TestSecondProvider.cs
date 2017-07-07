// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestSecondProvider.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The second provider to test multiple provider configurations
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.API.Tests.Mock
{
    using JetBrains.Annotations;

    using KlusterKite.API.Attributes;
    using KlusterKite.API.Provider;

    /// <summary>
    /// The second provider to test multiple provider configurations
    /// </summary>
    [ApiDescription(Name = "TestSecondApi")]
    public class TestSecondProvider : ApiProvider
    {
        /// <summary>
        /// Gets the test field
        /// </summary>
        [DeclareField]
        [UsedImplicitly]
        public string SecondApiName => "SecondApiName";
    }
}
