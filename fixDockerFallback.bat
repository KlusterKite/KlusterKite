NuGet.exe "Install" "ClusterKit.NodeManager.FallbackPackageFixer" "-OutputDirectory" "packages" "-ExcludeVersion" -Prerelease -Version 0.0.0.0-local

del Docker\ClusterKitManager\fallBackConfiguration.json
copy Docker\ClusterKitManager\fallBackConfiguration.orig Docker\ClusterKitManager\fallBackConfiguration.json
packages\ClusterKit.NodeManager.FallbackPackageFixer\tools\net46\ClusterKit.NodeManager.FallbackPackageFixer.exe Docker\ClusterKitManager\fallBackConfiguration.json packageOut packageThirdPartyDir 

del Docker\ClusterKitPublisher\fallBackConfiguration.json
copy Docker\ClusterKitPublisher\fallBackConfiguration.orig Docker\ClusterKitPublisher\fallBackConfiguration.json
packages\ClusterKit.NodeManager.FallbackPackageFixer\tools\net46\ClusterKit.NodeManager.FallbackPackageFixer.exe Docker\ClusterKitPublisher\fallBackConfiguration.json packageOut packageThirdPartyDir 