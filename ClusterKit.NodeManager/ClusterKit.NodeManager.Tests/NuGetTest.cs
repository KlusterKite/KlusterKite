using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClusterKit.NodeManager.Tests
{
    using NuGet;

    using Xunit;
    using Xunit.Abstractions;

    public class NuGetTest
    {
        /// <summary>
        /// XUnit output stream
        /// </summary>
        private readonly ITestOutputHelper output;

        /// <summary>
        /// Initializes a new instance of the <see cref="NuGetTest"/> class.
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        public NuGetTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        //[Fact]
        public void Test()
        {
            var feedUrl = "http://192.168.99.100:81/";

            var nugetRepository = PackageRepositoryFactory.Default.CreateRepository(feedUrl);

            var newPackages = nugetRepository.Search("", true).Where(p => p.IsLatestVersion).ToList();
            foreach (var package in newPackages)
            {
                this.output.WriteLine($"{package.Id} {package.Version}");
            }
        }
    }
}