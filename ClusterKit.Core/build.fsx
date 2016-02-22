#r "../packages/FAKE/tools/FakeLib.dll" // include Fake lib
open Fake
// Properties
let buildDir = "./build/"
let buildMode = getBuildParamOrDefault "buildMode" "Release"
let buildParams defaults =
        { defaults with
            Verbosity = Some(Quiet)
            Targets = ["Build"]
            Properties =
                [
                    "DebugSymbols", "True"
                    "Configuration", buildMode
                ]
         }

Target "BuildApp" (fun _ ->
    build buildParams "./ClusterKit.Core/ClusterKit.Core/ClusterKit.Core.csproj"
      |> DoNothing
)

Target "Deploy" (fun _ ->
    trace "Heavy deploy action"
)

"BuildApp"
   ==> "Deploy"

Run "Deploy"