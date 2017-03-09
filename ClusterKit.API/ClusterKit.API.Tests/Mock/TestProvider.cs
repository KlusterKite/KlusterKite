// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestProvider.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Test api provider
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Tests.Mock
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using ClusterKit.API.Client;
    using ClusterKit.API.Client.Attributes;
    using ClusterKit.API.Client.Converters;
    using ClusterKit.API.Provider;
    using ClusterKit.Security.Client;

    using JetBrains.Annotations;

    using Newtonsoft.Json.Linq;

    using Xunit;

    /// <summary>
    /// Test api provider
    /// </summary>
    [UsedImplicitly]
    [ApiDescription(Description = "Tested API", Name = "TestApi")]
    public class TestProvider : ApiProvider
    {
        /// <summary>
        /// The test objects connection
        /// </summary>
        private readonly TestObjectConnection connection;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestProvider"/> class.
        /// </summary>
        /// <param name="objects">
        /// The list of pre-stored objects.
        /// </param>
        public TestProvider(List<TestObject> objects = null)
        {
            if (objects == null)
            {
                objects = new List<TestObject>();
            }

            this.connection = new TestObjectConnection(objects.ToDictionary(o => o.Id));
        }

        /// <summary>
        /// Test enum
        /// </summary>
        [ApiDescription(Description = "The test enum", Name = "EnTest")]
        public enum EnTest
        {
            /// <summary>
            /// Test enum item1
            /// </summary>
            EnumItem1 = 1,

            /// <summary>
            /// Test enum item2
            /// </summary>
            EnumItem2 = 2
        }

        /// <summary>
        /// Test enum
        /// </summary>
        [ApiDescription(Description = "The test flags")]
        [Flags]
        public enum EnFlags
        {
            /// <summary>
            /// Test enum item1
            /// </summary>
            FlagsItem1 = 1,

            /// <summary>
            /// Test enum item2
            /// </summary>
            FlagsItem2 = 2
        }

        /// <summary>
        /// Async scalar field
        /// </summary>
        [DeclareField]
        [UsedImplicitly]
        public Task<List<decimal>> AsyncArrayOfScalarField => Task.FromResult(new List<decimal> { 4M, 5M });

        /// <summary>
        /// Gets the forwarded scalar property
        /// </summary>
        [DeclareField(ReturnType = typeof(string))]
        [UsedImplicitly]
        public Task<JValue> AsyncForwardedScalar => Task.FromResult(new JValue("AsyncForwardedScalar"));

        /// <summary>
        /// Async nested provider
        /// </summary>
        [DeclareField(Name = "nestedAsync")]
        [UsedImplicitly]
        public Task<NestedProvider> AsyncNestedProvider => Task.FromResult(new NestedProvider());

        /// <summary>
        /// Async scalar field
        /// </summary>
        [DeclareField]
        [UsedImplicitly]
        public Task<string> AsyncScalarField => Task.FromResult("AsyncScalarField");

        /// <summary>
        /// Test objects connection
        /// </summary>
        [DeclareConnection(CanCreate = true, CanDelete = true, CanUpdate = true)]
        [UsedImplicitly]
        public TestObjectConnection Connection => this.connection;

        /// <summary>
        /// Gets a value indicating whether something is faulting
        /// </summary>
        [DeclareField]
        [UsedImplicitly]
        public bool FaultedSyncField
        {
            get
            {
                throw new Exception("test");
            }
        }

        /// <summary>
        /// Gets the forwarded array of scalars
        /// </summary>
        [DeclareField(ReturnType = typeof(int[]))]
        [UsedImplicitly]
        public JArray ForwardedArray => new JArray(new[] { 5, 6, 7 });

        /// <summary>
        /// Async nested provider
        /// </summary>
        [DeclareField]
        [UsedImplicitly]
        public int[] SyncArrayOfScalarField => new[] { 1, 2, 3 };

        /// <summary>
        /// Sync nested provider
        /// </summary>
        [DeclareField(Name = "nestedSync")]
        [UsedImplicitly]
        public NestedProvider SyncNestedProvider => new NestedProvider();

        /// <summary>
        /// Sync scalar field
        /// </summary>
        [DeclareField]
        [UsedImplicitly]
        public string SyncScalarField => "SyncScalarField";

        /// <summary>
        /// Sync scalar field
        /// </summary>
        [DeclareField]
        [UsedImplicitly]
        public EnTest SyncEnumField => EnTest.EnumItem1;

        /// <summary>
        /// Sync scalar field
        /// </summary>
        [DeclareField]
        [UsedImplicitly]
        public EnFlags SyncFlagsField => EnFlags.FlagsItem1;

        /// <summary>
        /// Gets or sets the third party object
        /// </summary>
        [DeclareField(Converter = typeof(StringConverter))]
        [UsedImplicitly]
        public ThirdPartyObject ThirdParty { get; set; } = new ThirdPartyObject { Contents = "Third party" };

        /// <summary>
        /// Gets or sets the list of third party objects
        /// </summary>
        [DeclareField(Converter = typeof(ArrayConverter<StringConverter, string>))]
        [UsedImplicitly]
        public List<ThirdPartyObject> ThirdParties { get; set; }

        /// <summary>
        /// Some public method
        /// </summary>
        /// <param name="intArg">
        /// Integer parameter
        /// </param>
        /// <param name="stringArg">
        /// String parameter
        /// </param>
        /// <param name="objArg">
        /// Object parameter
        /// </param>
        /// <param name="intArrayArg">
        /// The integer Array parameter.
        /// </param>
        /// <param name="requestContext">
        /// The request context
        /// </param>
        /// <param name="apiRequest">
        /// The sub-request
        /// </param>
        /// <returns>
        /// The test string
        /// </returns>
        [DeclareField]
        [UsedImplicitly]
        public Task<NestedProvider> AsyncObjectMethod(
            int intArg,
            string stringArg,
            NestedProvider objArg,
            int[] intArrayArg,
            RequestContext requestContext,
            ApiRequest apiRequest)
        {
            Assert.Equal(1, intArg);
            Assert.Equal("test", stringArg);
            Assert.NotNull(objArg);
            Assert.Equal("nested test", objArg.SyncScalarField);

            Assert.NotNull(intArrayArg);
            Assert.Equal(3, intArrayArg.Length);
            Assert.Equal(7, intArrayArg[0]);
            Assert.Equal(8, intArrayArg[1]);
            Assert.Equal(9, intArrayArg[2]);

            Assert.NotNull(requestContext);
            Assert.NotNull(apiRequest);
            Assert.Equal("asyncObjectMethod", apiRequest.FieldName);

            return Task.FromResult(new NestedProvider { SyncScalarField = "returned type" });
        }

        /// <summary>
        /// Faulted async method
        /// </summary>
        /// <returns>Faulted task</returns>
        [DeclareField]
        [UsedImplicitly]
        public Task<NestedProvider> FaultedASyncMethod()
        {
            return Task.FromException<NestedProvider>(new Exception("test exception"));
        }

        /// <summary>
        /// Some public method
        /// </summary>
        /// <param name="intArg">Integer parameter</param>
        /// <param name="stringArg">String parameter</param>
        /// <param name="objArg">Object parameter</param>
        /// <param name="requestContext">The request context</param>
        /// <param name="apiRequest">The sub-request</param>
        /// <returns>The test string</returns>
        [DeclareField]
        [UsedImplicitly]
        public string SyncScalarMethod(
            int intArg,
            string stringArg,
            NestedProvider objArg,
            RequestContext requestContext,
            ApiRequest apiRequest)
        {
            Assert.Equal(1, intArg);
            Assert.Equal("test", stringArg);
            Assert.NotNull(objArg);
            Assert.Equal("nested test", objArg.SyncScalarField);

            Assert.NotNull(requestContext);
            Assert.NotNull(apiRequest);
            Assert.Equal("syncScalarMethod", apiRequest.FieldName);

            return "ok";
        }
    }
}