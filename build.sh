export MONO_THREADS_PER_CPU=2048
mono --runtime=v4.0 nuget.exe "Install" "FAKE" "-OutputDirectory" "packages" "-ExcludeVersion"
mono --runtime=v4.0 packages/FAKE/tools/FAKE.exe build.fsx
