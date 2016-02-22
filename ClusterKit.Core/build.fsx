#r "packages/FAKE/tools/FakeLib.dll" // include Fake lib
open Fake 

// Properties
let buildDir = "./build/"

Target "BuildApp" (fun _ ->
    !! "./**/*.csproj"
      |> MSBuildRelease buildDir "Build"
      |> Log "AppBuild-Output: "
)

Target "Deploy" (fun _ ->
    trace "Heavy deploy action"
)

"BuildApp" 
   ==> "Deploy"

Run "Deploy"