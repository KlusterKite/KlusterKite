NuGet.exe "Install" "FAKE" "-OutputDirectory" "packages" "-ExcludeVersion"
packages\FAKE\tools\FAKE.exe build.fsx Test -ev version 0.0.0.0-local
