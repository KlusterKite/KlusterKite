NuGet.exe "Install" "FAKE" "-OutputDirectory" "packages" "-ExcludeVersion"
packages\FAKE\tools\FAKE.exe build.fsx ReloadNuGet -ev version 0.0.0.0-local
