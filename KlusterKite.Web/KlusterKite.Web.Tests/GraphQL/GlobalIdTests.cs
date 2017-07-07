// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GlobalIdTests.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Testing the <see cref="GlobalIdSerializer" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.Tests.GraphQL
{
    using ClusterKit.Web.GraphQL.Publisher;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Testing the <see cref="GlobalIdSerializer"/>
    /// </summary>
    public class GlobalIdTests
    {
        /// <summary>
        /// The output.
        /// </summary>
        private readonly ITestOutputHelper output;

        /// <summary>
        /// Initializes a new instance of the <see cref="GlobalIdTests"/> class.
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        public GlobalIdTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        /// <summary>
        /// Testing the <see cref="GlobalIdSerializer.PackGlobalId"/>
        /// </summary>
        [Fact]
        public void PackTest()
        {
            var globalId = "{\"p\":[{\"f\":\"connection\"}],\"api\":\"TestApi\",\"id\":\"67885ba0-b284-438f-8393-ee9a9eb299d1\"}";
            var json = JsonConvert.DeserializeObject(globalId) as JObject;
            var packed = json.PackGlobalId();
            this.output.WriteLine(packed);

            var unpacked = packed.UnpackGlobalId();
            Assert.Equal(globalId, unpacked);
        }
    }
}
