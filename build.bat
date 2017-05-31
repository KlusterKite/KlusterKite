IF NOT EXIST packages\FAKE\tools\FAKE.exe (
	nuget.exe "Install" "FAKE" "-OutputDirectory" "packages" "-ExcludeVersion"                        
)

IF NOT EXIST packages\NuGet.Protocol.Core.v3 (
	nuget.exe "Install" "NuGet.Protocol.Core.v3" "-OutputDirectory" "packages" "-ExcludeVersion"                        
)


packages\FAKE\tools\FAKE.exe build.fsx %1
