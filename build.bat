NuGet.exe "Install" "FAKE" "-OutputDirectory" "packages" "-ExcludeVersion"
packages\FAKE\tools\FAKE.exe build.fsx %1
