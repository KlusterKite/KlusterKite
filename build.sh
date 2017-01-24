export MONO_THREADS_PER_CPU=2048

if [ ! -f packages/FAKE/tools/FAKE.exe ]; then
	mono --runtime=v4.0 nuget.exe "Install" "FAKE" "-OutputDirectory" "packages" "-ExcludeVersion"
fi

mono --runtime=v4.0 packages/FAKE/tools/FAKE.exe build.fsx $1
