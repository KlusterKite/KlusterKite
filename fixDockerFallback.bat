NuGet.exe "Install" "ClusterKit.NodeManager.FallbackPackageDependencyFixer" "-OutputDirectory" "packages" "-ExcludeVersion" -Prerelease -Version 0.0.0.0-local

del Docker\ClusterKitManager\fallBackConfiguration.json
copy Docker\ClusterKitManager\fallBackConfiguration.orig Docker\ClusterKitManager\fallBackConfiguration.json
packages\ClusterKit.NodeManager.FallbackPackageDependencyFixer\tools\net46\ClusterKit.NodeManager.FallbackPackageDependencyFixer.exe Docker\ClusterKitManager\fallBackConfiguration.json packages packageOut

del Docker\ClusterKitPublisher\fallBackConfiguration.json
copy Docker\ClusterKitPublisher\fallBackConfiguration.orig Docker\ClusterKitPublisher\fallBackConfiguration.json
packages\ClusterKit.NodeManager.FallbackPackageDependencyFixer\tools\net46\ClusterKit.NodeManager.FallbackPackageDependencyFixer.exe Docker\ClusterKitPublisher\fallBackConfiguration.json packages packageOut