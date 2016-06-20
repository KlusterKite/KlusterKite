# Instructions to build solution

There is global build script that does all build related tasks - `build.fsx`.
At first you need build utility - FAKE (it is not contained in this solution)
To obtain it run `NuGet.exe "Install" "FAKE" "-OutputDirectory" "packages" "-ExcludeVersion"` or `mono NuGet.exe "Install" "FAKE" "-OutputDirectory" "packages" "-ExcludeVersion"`

Now you can call build scripts like:
`packages\FAKE\tools\FAKE.exe build.fsx [target]` or `mono packages\FAKE\tools\FAKE.exe build.fsx [target]`

## Targets

All targets are divided into two groups:
* single task targets, that do only one task and usually it is required that some other task should be done previously
* Complex task targets - that do all tasks in right order to achieve final task

Normally one shall run *Complex task targets*
Default target (if omitted) is *FinalRefreshLocalDependencies*


### Single task targets

#### Clean
Remove contents of all temp directories used during the build

#### Build
Compiles all projects (with respect to all dependencies) and places output to `build/tmp` (complete build result) and `build/clean` (just library itself)
All assemblies are marked with default version `0.0.0-local`, if `SetVersion` target was not called in this session. In that case, they are marked as newest possible version according to the local cluster NuGet server.

#### CreateNuGet
Creates NuGet packages from previous built projects.
All packages are marked with default version `0.0.0-local`, if `SetVersion` target was not called in this session. In that case, they are marked as newest possible version according to the local cluster NuGet server.
All packages are placed into the `packageOut` directory

#### RefreshLocalDependencies
Removes all content from `packages` directory and performs full package restore procedure for every project

#### Test
Runs all specified xunit tests from previously built projects

#### DockerBase
Performs the build of base docker images. Base docker images do not contain modified project files and are not intended for future change (or frequent changes).

#### DockerContainers
Preforms the build of the rest docker images. They can contain service launcher and cache of currently used packages for faster start (that are all packages from `packages` folder, excluding ones, that are also contained in `packageOut` directory).

#### CleanDockerImages
Removes unnamed images from local docker. This type of images usually appears during subsequent builds of images, when one image replaces the other.

#### PushThirdPartyPackages
Pushes all NuGet packages from solution cache (`packages` folder) to local cluster NuGet server, excluding ones, that are also contained in `packageOut` directory

#### PushLocalPackages
Pushes all created NuGet packages from `packageOut` directory

#### SetVersion
Asks current cluster NuGet server for the latest version of `ClusterKit.Core` library and assigns next version for subsequent build or package creation task.

#### CleanPackageCache
The same as `RefreshLocalDependencies`. TODO: remove duplicate


### Complex task targets

#### FinalBuild
Builds all projects
Performs *Clean* -> *Build*

#### FinalTest
Builds all projects and runs all XUnit test
Performs *Clean* -> *Build* -> *Test*

#### FinalCreateNuGet
Creates nuget packages from projects
Performs *Clean* -> *Build* -> *CreateNuGet*

#### FinalBuildDocker
Builds all docker images
Performs *CleanPackageCache* -> *DockerBase* -> *DockerContainers* -> *CleanDockerImages*

#### FinalPushLocalPackages
Builds NuGet packages and sends them to local cluster NuGet server.
This is usually done to update current cluster.
Performs *Clean* -> *SetVersion* -> *Build* -> *CreateNuGet* -> *PushLocalPackages*

#### FinalPushAllPackages
Builds local packages and sends them to local cluster NuGet server along with the cache packages.
This should be done on an empty cluster or after third party packages update / install.
Performs *Clean* -> *SetVersion* -> *Build* -> *CreateNuGet* -> *PushLocalPackages* -> *PushThirdPartyPackages*

#### FinalRefreshLocalDependencies
Rebuilds all projects and reinstalls local dependent packages
This is usually done when one modifies several solutions with indirect dependencies to introduce changes from one solution to another
Performs *Clean* -> *Build* -> *CreateNuGet* -> *CleanPackageCache* -> *RefreshLocalDependencies*

### Special dev-time tasks

#### CreateGlobalSolution
Creates special global solution(`global.sln`) in build directory with all defined projects in it (useful while global third-party libs upgrade or other batch edits)
