#r ".fake/build.fsx/packages/netcorebuild/Fake.Core.Environment/lib/netstandard1.6/Fake.Core.Environment.dll"
namespace KlusterKite.Build

open System.IO
open Fake.Core.Environment

module Config =

    let testPackageName = "KlusterKite.Core"
    let buildDir = Path.GetFullPath("./build")
    let mutable packageDir = Path.GetFullPath("./packageOut")
    let packagePushDir = Path.GetFullPath("./packagePush")
    let packageThirdPartyDir = Path.GetFullPath("./packageThirdPartyDir")

    let envVersion = environVarOrDefault "version" null
    let mutable version = if envVersion <> null then envVersion else "0.0.0-local"