export MONO_THREADS_PER_CPU=2048

IF NOT EXIST packages\FAKE\tools\FAKE.exe (
	mono nuget.exe "Install" "FAKE" "-OutputDirectory" "packages" "-ExcludeVersion"                        
)

mono packages\FAKE\tools\FAKE.exe build.fsx %1