mono --runtime=v4.0 nuget.exe "Install" "FAKE" "-OutputDirectory" "packages" "-ExcludeVersion"
mono --runtime=v4.0 packages/FAKE/tools/FAKE.exe build.fsx ReloadNuGet -ev version 0.0.0.0-local