namespace KlusterKite.NodeManager.Tests
{
    using System.IO;
    using System.Linq;

    using Akka.Configuration;

    using KlusterKite.NodeManager.Launcher.Messages;
    using KlusterKite.NodeManager.Launcher.Utils;

    using Newtonsoft.Json;

    using NuGet.Packaging.Core;
    using NuGet.Versioning;

    using Xunit;
    using Xunit.Abstractions;

    public class TempTest
    {
        private readonly ITestOutputHelper output;

        /// <summary>
        /// Initializes a new instance of the <see cref="TempTest"/> class.
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        public TempTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void LauncherTest()
        {
            var config = ConfigurationFactory.ParseString(File.ReadAllText("./TempData/config.hocon"));
            var configuration =
                JsonConvert.DeserializeObject<NodeStartUpConfiguration>(File.ReadAllText("./TempData/fallBackConfiguration.json"));
            var nugetUrl = "http://nuget/";
            var serviceDir = Path.Combine(".", "service");
            if (Directory.Exists(serviceDir))
            {
                Directory.Delete(serviceDir, true);
            }

            Directory.CreateDirectory(serviceDir);

            var repository = new RemotePackageRepository(nugetUrl);
            repository.CreateServiceAsync(
                configuration.Packages.Select(p => new PackageIdentity(p.Id, NuGetVersion.Parse(p.Version))),
                "win7-x64",
                PackageRepositoryExtensions.CurrentRuntime,
                serviceDir,
                "KlusterKite.Core.Service",
                this.output.WriteLine).GetAwaiter().GetResult();
        }

    }
}
