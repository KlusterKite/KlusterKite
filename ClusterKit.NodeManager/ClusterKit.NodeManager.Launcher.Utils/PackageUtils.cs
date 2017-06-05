// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageUtils.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Helper methods to work with nuget packages
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Launcher.Utils
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using ClusterKit.NodeManager.Launcher.Utils.Exceptions;

    using NuGet.Common;
    using NuGet.Configuration;
    using NuGet.Frameworks;
    using NuGet.Packaging.Core;
    using NuGet.Protocol;
    using NuGet.Protocol.Core.Types;
    using NuGet.Versioning;

    /// <summary>
    /// Helper methods to work with nuget packages
    /// </summary>
    public static class PackageUtils
    {
        /// <summary>
        /// Installs specified packages to specified place
        /// </summary>
        /// <param name="packages">The list of packages to install</param>
        /// <param name="framework">The target framework to install</param>
        /// <param name="nugetUrl">The url of nuget repository</param>
        /// <param name="directoryToInstall">The directory to install packages contents</param>
        public static void Install(
            this IEnumerable<PackageIdentity> packages,
            NuGetFramework framework,
            string nugetUrl,
            string directoryToInstall)
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);

            var source = new PackageSource(nugetUrl);
            var sourceRepository = Repository.Factory.GetCoreV3(source.Source);
            var downloadResource = sourceRepository.GetResource<DownloadResource>();
            var packageDownloadContext = new PackageDownloadContext(new SourceCacheContext());

            try
            {
                var downloadTasks = packages.Select(
                    package => downloadResource.GetDownloadResourceResultAsync(
                        package,
                        packageDownloadContext,
                        tempDir,
                        NullLogger.Instance,
                        CancellationToken.None));
                var downloads = Task.WhenAll(downloadTasks).GetAwaiter().GetResult();

                var tools = downloads.Where(d => !d.PackageReader.GetLibItems().Any());
                var libraries = downloads.Where(d => d.PackageReader.GetLibItems().Any());

                foreach (var package in tools.OrderBy(d => d.PackageReader.GetIdentity().Id))
                {
                    Console.WriteLine(package.PackageReader.GetIdentity());
                    ExtractPackage(package, framework, directoryToInstall);
                }

                foreach (var package in libraries.OrderBy(d => d.PackageReader.GetIdentity().Id))
                {
                    Console.WriteLine(package.PackageReader.GetIdentity());
                    ExtractPackage(package, framework, directoryToInstall);
                }
            }
            finally
            {
                try
                {
                    GC.Collect();
                    Directory.Delete(tempDir, true);
                }
                catch
                {
                    // ignore
                }
            }
        }

        /// <summary>
        /// Searches for a specific package
        /// </summary>
        /// <param name="nugetUrl">
        /// The nuget server address
        /// </param>
        /// <param name="id">
        /// The package id
        /// </param>
        /// <param name="version">
        /// The package version.
        /// </param>
        /// <returns>
        /// The package metadata
        /// </returns>
        public static Task<IPackageSearchMetadata> Search(string nugetUrl, string id, NuGetVersion version)
        {
            var source = new PackageSource(nugetUrl);
            var sourceRepository = Repository.Factory.GetCoreV3(source.Source);
            var listResource = sourceRepository.GetResource<ListResource>();
            return listResource.Search(id, version);
        }

        /// <summary>
        /// Searches for a specific package
        /// </summary>
        /// <param name="listResource">
        /// The resource to search
        /// </param>
        /// <param name="id">
        /// The package id
        /// </param>
        /// <param name="version">
        /// The package version.
        /// </param>
        /// <returns>
        /// The package metadata
        /// </returns>
        public static async Task<IPackageSearchMetadata> Search(
            this ListResource listResource,
            string id,
            NuGetVersion version)
        {
            var packages = await listResource.ListAsync(
                               id,
                               true,
                               true,
                               false,
                               NullLogger.Instance,
                               CancellationToken.None);
            var enumerator = packages.GetEnumeratorAsync();
            while (await enumerator.MoveNextAsync())
            {
                if (enumerator.Current.Identity.Id == id && enumerator.Current.Identity.Version == version)
                {
                    return enumerator.Current;
                }
            }

            return null;
        }

        /// <summary>
        /// Get the list of package descriptions
        /// </summary>
        /// <param name="repository">The package repository</param>
        /// <param name="terms">The search terms</param>
        /// <returns>The list of package descriptions</returns>
        public static async Task<List<IPackageSearchMetadata>> Search(string repository, string terms)
        {
            var source = new PackageSource(repository);
            var sourceRepository = Repository.Factory.GetCoreV3(source.Source);
            var resource = sourceRepository.GetResource<PackageSearchResource>();
            var result = new List<IPackageSearchMetadata>();

            const int PageSize = 1000;
            var position = 0;
            while (true)
            {
                var searchMetadata = await resource.SearchAsync(
                                          terms,
                                          new SearchFilter(true, null),
                                          position,
                                          PageSize,
                                          NullLogger.Instance,
                                          CancellationToken.None);
                var previousCount = result.Count;
                result.AddRange(searchMetadata);
                position += PageSize;
                if (result.Count - previousCount < PageSize)
                {
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Searches the repository for specified packages and their dependencies
        /// </summary>
        /// <param name="packages">The list of packages id</param>
        /// <param name="framework">The framework to check dependencies for</param>
        /// <param name="nugetUrl">The url of nuget repository</param>
        /// <returns>The list of found packages</returns>
        /// <exception cref="PackageNotFoundException">In case of package or it's dependency is missing</exception>
        public static IEnumerable<IPackageSearchMetadata> SearchLatestPackagesWithDependencies(
            this IEnumerable<string> packages,
            NuGetFramework framework,
            string nugetUrl)
        {
            var source = new PackageSource(nugetUrl);
            var sourceRepository = Repository.Factory.GetCoreV3(source.Source);
            var listResource = sourceRepository.GetResource<ListResource>();

            var requiredPackages = Task.WhenAll(
                packages.Select(
                    async id =>
                        {
                            var result = await listResource.Search(id);
                            if (result == null)
                            {
                                throw new PackageNotFoundException(id);
                            }

                            return result;
                        })).GetAwaiter().GetResult().ToDictionary(p => p.Identity.Id.ToLower());

            while (true)
            {
                var requirements = requiredPackages.Values
                    .Select(
                        p => new
                                 {
                                     DependencySet = NuGetFrameworkUtility.GetNearest(p.DependencySets, framework),
                                     Source = p
                                 }).Where(
                        ds => ds.DependencySet != null && (ds.DependencySet.TargetFramework == null
                                                           || NuGetFrameworkUtility.IsCompatibleWithFallbackCheck(
                                                               framework,
                                                               ds.DependencySet.TargetFramework)))
                    .SelectMany(ds => ds.DependencySet.Packages).GroupBy(ds => ds.Id);

                var additionalPackagesTasks = requirements.Select(
                    async r =>
                        {
                            if (requiredPackages.TryGetValue(r.Key.ToLower(), out var currentPackage) && r.All(
                                    rc => rc.VersionRange.Satisfies(currentPackage.Identity.Version)))
                            {
                                return currentPackage;
                            }

                            var dependenciesList = await listResource.ListAsync(
                                                       r.Key,
                                                       true,
                                                       true,
                                                       false,
                                                       NullLogger.Instance,
                                                       CancellationToken.None);

                            var enumerator = dependenciesList.GetEnumeratorAsync();
                            var possiblePackages = new List<IPackageSearchMetadata>();
                            while (await enumerator.MoveNextAsync())
                            {
                                var p = enumerator.Current;
                                if (r.Key == enumerator.Current.Identity.Id
                                    && r.All(rq => rq.VersionRange.Satisfies(p.Identity.Version)))
                                {
                                    possiblePackages.Add(p);
                                }
                            }

                            var dependency = possiblePackages.OrderBy(p => p.Identity.Version).FirstOrDefault();

                            if (dependency == null)
                            {
                                throw new PackageNotFoundException(r.Key);
                            }

                            if (dependency.Identity.Id != r.Key)
                            {
                                Console.WriteLine($"!!! {dependency.Identity.Id} != {r.Key}");
                            }

                            return dependency;
                        });

                var additionalPackages = Task.WhenAll(additionalPackagesTasks).GetAwaiter().GetResult().ToList();
                additionalPackages.RemoveAll(
                    p => requiredPackages.TryGetValue(p.Identity.Id.ToLower(), out var currentPackage)
                         && currentPackage == p);

                if (additionalPackages.Count == 0)
                {
                    break;
                }

                foreach (var package in additionalPackages)
                {
                    requiredPackages[package.Identity.Id.ToLower()] = package;
                }
            }

            return requiredPackages.Values;
        }

        /// <summary>
        /// Extracts the lib files to execution directory
        /// </summary>
        /// <param name="package">The package to extract</param>
        /// <param name="frameworkName">The current framework name</param>
        /// <param name="executionDir">The execution directory to load packages</param>
        private static void ExtractPackage(
            DownloadResourceResult package,
            NuGetFramework frameworkName,
            string executionDir)
        {
            try
            {
                var files = NuGetFrameworkUtility.GetNearest(package.PackageReader.GetLibItems(), frameworkName)
                            ?? NuGetFrameworkUtility.GetNearest(package.PackageReader.GetToolItems(), frameworkName);

                if (files == null)
                {
                    return;
                }

                foreach (var file in files.Items)
                {
                    using (var fileStream = File.Create(Path.Combine(executionDir, Path.GetFileName(file) ?? file)))
                    {
                        package.PackageReader.GetStream(file).CopyTo(fileStream);
                    }
                }
            }
            finally
            {
                package.Dispose();
            }
        }

        /// <summary>
        /// Searches for a specific package
        /// </summary>
        /// <param name="listResource">The resource to search</param>
        /// <param name="id">The package id</param>
        /// <returns>The package metadata</returns>
        private static async Task<IPackageSearchMetadata> Search(this ListResource listResource, string id)
        {
            var packages = await listResource.ListAsync(
                               id,
                               true,
                               false,
                               false,
                               NullLogger.Instance,
                               CancellationToken.None);
            var enumerator = packages.GetEnumeratorAsync();
            while (await enumerator.MoveNextAsync())
            {
                if (enumerator.Current.Identity.Id == id)
                {
                    return enumerator.Current;
                }
            }

            return null;
        }
    }
}