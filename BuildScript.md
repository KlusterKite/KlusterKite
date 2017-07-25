# Instructions to build solution

There is global build script that does all build related tasks - `build.fsx`.
At first, you need build utility - FAKE (it is not contained in this solution). Please check the [FAKE](https://fake.build/) documentation for installation instructions.


Now you can call build scripts like:
`fake run -t [target] build.fsx`

## Targets

All targets are divided into two groups:
* single task targets, that do only one task and usually it is required that some other task should be done previously
* Complex task targets - that do all tasks in right order to achieve final task

Normally one shall run *Complex task targets*
Default target (if omitted) is *FinalRefreshLocalDependencies*


### Single task targets

#### Clean
Remove contents of all temp directories used during the build

#### PrepareSources
Copies all sources to the `./build/src` directory and adjusts the assembly version. All assemblies are marked with default version `0.0.0-local`, if `SetVersion` target was not called in this session. In that case, they are marked as newest possible version according to the local cluster NuGet server.

#### Build
Compiles all projects from `./build/src` directory.

#### Nuget
Creates NuGet packages from previously built projects.
All packages are placed into the `packageOut` directory if there were no `SetVersion`, otherwise, it places packages to the `packagePush` folder

#### Tests
Runs all specified xunit tests from previously built projects

#### DockerBase
Performs the build of base docker images. Base docker images do not contain modified project files and are not intended for future change (or frequent changes).

#### DockerContainers
Preforms the build of the rest docker images. They can contain service launcher and a cache of currently used packages for a faster start (that are all packages from `packages` folder, excluding ones, that are also contained in `packageOut` directory).

#### CleanDockerImages
Removes unnamed images from local docker. This type of images usually appears during subsequent builds of images, when one image replaces the other.

### RestoreThirdPartyPackages
Checks the solution for used third party packages and copies the .nipckg files to the `./packageThirdPartyDir` folder.

#### PushThirdPartyPackages
Pushes all third-party NuGet packages from the `packageThirdPartyDir` folder to local cluster NuGet server. The push is made with a single command for all packages in the folder. This is the fastest way, but it will fail in case any package push failed.

#### RePushThirdPartyPackages
Pushes all third-party NuGet packages from the `packageThirdPartyDir` folder to local cluster NuGet server. The push is made with a separate command for every package in the folder. This is slower then `PushThirdPartyPackages`, but it will continue pushing even if it encounters a push error.

#### PushLocalPackages
Pushes all created NuGet packages from `packagePush` folder to local cluster NuGet server.  The push is made with single command for all packages in the folder. This is the fastest way, but it will fail in case any package push failed.

#### RePushLocalPackages
Pushes all created NuGet packages from `packagePush` folder to local cluster NuGet server.  The push is made with a separate command for every package in the folder. This is slower then `PushThirdPartyPackages`, but it will continue pushing even if it encounters a push error.

#### SetVersion
Asks current cluster NuGet server for the latest version of `KlusterKite.Core` library and assigns next version for subsequent build or package creation task.

#### CleanPackageCache
The same as `RefreshLocalDependencies`. TODO: remove duplicate


### Complex task targets

#### FinalBuild
Builds all projects
Performs *Clean* -> *Build*

#### FinalBuildDocker
Builds all docker images
Performs *CleanPackageCache* -> *DockerBase* -> *DockerContainers* -> *CleanDockerImages*

#### FinalPushLocalPackages
Builds NuGet packages and sends them to local cluster NuGet server.
This is usually done to update current cluster.
Performs *Clean* -> *SetVersion* -> *Build* -> *Nuget* -> *PushLocalPackages*

#### FinalPushAllPackages
Builds local packages and sends them to local cluster NuGet server along with the cache packages.
This should be done on an empty cluster or after third party packages update / install.
Performs *Clean* -> *SetVersion* -> *Build* -> *Nuget* -> *PushLocalPackages* -> *PushThirdPartyPackages*