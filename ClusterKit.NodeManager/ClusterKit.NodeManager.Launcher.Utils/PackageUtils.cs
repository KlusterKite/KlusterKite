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
    using NuGet.Packaging;
    using NuGet.Packaging.Core;
    using NuGet.Protocol;
    using NuGet.Protocol.Core.Types;

    /// <summary>
    /// Helper methods to work with nuget packages
    /// </summary>
    public static class PackageUtils
    {
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

            var requiredPackages = packages.Select(id =>
                {
                    var result = listResource.Search(id).GetAwaiter().GetResult();
                    if (result == null)
                    {
                        throw new PackageNotFoundException(id);
                    }

                    return result;
                }).ToList();

            while (true)
            {
                var additionalPackages = requiredPackages
                    .Select(
                        p => new
                                 {
                                     DependencySet =
                                     NuGetFrameworkUtility.GetNearest(p.DependencySets, framework),
                                     Source = p
                                 }).Where(
                        ds => ds.DependencySet != null
                              && (ds.DependencySet.TargetFramework == null
                                  || NuGetFrameworkUtility.IsCompatibleWithFallbackCheck(
                                      framework,
                                      ds.DependencySet.TargetFramework)))
                    .SelectMany(ds => ds.DependencySet.Packages.Select(p => new { ds.Source, Package = p }))
                    .GroupBy(ds => ds.Package.Id).Where(r => requiredPackages.All(p => p.Identity.Id != r.Key))
                    .Select(
                        r =>
                            {
                                var dependency = listResource.SearchAll(r.Key)
                                    .Where(
                                        p => r.All(rq => rq.Package.VersionRange.Satisfies(p.Identity.Version)))
                                    .OrderBy(p => p.Identity.Version).FirstOrDefault();

                                if (dependency == null)
                                {
                                    throw new PackageNotFoundException(r.Key);
                                }

                                return dependency;
                            }).ToList();

                if (additionalPackages.Count == 0)
                {
                    break;
                }

                requiredPackages.AddRange(additionalPackages);
            }

            return requiredPackages;
        }

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
                Parallel.ForEach(
                    packages.OrderBy(r => r.Id),
                    package =>
                        {
                            var result = downloadResource.GetDownloadResourceResultAsync(
                                package,
                                packageDownloadContext,
                                tempDir,
                                NullLogger.Instance,
                                CancellationToken.None).Result;
                            if (result == null || result.Status != DownloadResourceResultStatus.Available)
                            {
                                throw new PackageNotFoundException(package.Id);
                            }

                            ExtractPackage(result.PackageReader, framework, directoryToInstall);
                        });
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
        /// Reads all <see cref="IEnumerableAsync{T}"/>  into plain list
        /// </summary>
        /// <typeparam name="T">The type of list</typeparam>
        /// <param name="enumerableAsync">The <see cref="IEnumerableAsync{T}"/></param>
        /// <returns>The list</returns>
        public static async Task<List<T>> ToList<T>(this IEnumerableAsync<T> enumerableAsync)
        {
            var enumerator = enumerableAsync.GetEnumeratorAsync();
            var list = new List<T>();
            while (await enumerator.MoveNextAsync())
            {
                list.Add(enumerator.Current);
            }

            return list;
        }

        /// <summary>
        /// Searches for a specific package
        /// </summary>
        /// <param name="listResource">The resource to search</param>
        /// <param name="id">The package id</param>
        /// <returns>The package metadata</returns>
        public static async Task<IPackageSearchMetadata> Search(this ListResource listResource, string id)
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

        /// <summary>
        /// Searches for a specific package
        /// </summary>
        /// <param name="listResource">The resource to search</param>
        /// <param name="id">The package id</param>
        /// <returns>The package metadata</returns>
        public static List<IPackageSearchMetadata> SearchAll(this ListResource listResource, string id)
        {
            var packages = listResource.ListAsync(
                id,
                true,
                true,
                false,
                NullLogger.Instance,
                CancellationToken.None).Result.ToList().Result;

            return packages.Where(p => p.Identity.Id == id).ToList();
        }

        /// <summary>
        /// Extracts the lib files to execution directory
        /// </summary>
        /// <param name="package">The package to extract</param>
        /// <param name="frameworkName">The current framework name</param>
        /// <param name="executionDir">The execution directory to load packages</param>
        private static void ExtractPackage(PackageReaderBase package, NuGetFramework frameworkName, string executionDir)
        {
            var files = NuGetFrameworkUtility.GetNearest(package.GetLibItems(), frameworkName)
                        ?? NuGetFrameworkUtility.GetNearest(package.GetToolItems(), frameworkName);

            if (files == null)
            {
                return;
            }

            foreach (var file in files.Items)
            {
                using (var fileStream = File.Create(Path.Combine(executionDir, Path.GetFileName(file) ?? file)))
                {
                    package.GetStream(file).CopyTo(fileStream);
                }
            }
        }
    }
}