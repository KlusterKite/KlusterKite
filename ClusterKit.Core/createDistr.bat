@echo off
set currentSolutionDir=%cd%

@SET buildCmd="C:\Program Files (x86)\MSBuild\14.0\Bin\amd64\msbuild.exe"

call :searchSln
goto :eof



:searchSln
for %%* in (.) do set CurrDirName=%%~nx*
if exist *.sln (
  set currentSolutionDir=%cd%
  if exist Build (
	echo Removing %cd%\Build
  	rmdir /S /Q Build
  )
  if exist Lib (
	echo Removing %cd%\Lib
  	rmdir /S /Q Lib
	mkdir Lib
  )

)

for %%f in (*.csproj) do (
	echo Creating build for %CurrDirName%
	%buildCmd% /t:Rebuild /p:Configuration=Release;OutDir=%currentSolutionDir%\Build\%CurrDirName% /nologo /noconlog /m %%f
	if exist %currentSolutionDir%\Build\%CurrDirName%\%CurrDirName%.dll (
		xcopy /Q %currentSolutionDir%\Build\%CurrDirName%\%CurrDirName%.dll %currentSolutionDir%\Lib\
	)
	
)

for /D %%d in (*) do (
    cd %%d
    call :searchSln
    cd ..
)
exit /b