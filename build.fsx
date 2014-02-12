#r "tools/FAKE/tools/FakeLib.dll"
open Fake
open System

RestorePackages()

// Properties
let buildDir = "./build/"
let testResultsDir = "./testresults/"
let buildMode = getBuildParamOrDefault "buildMode" "Release"

// Targets
Target "Clean" (fun _ ->
  CleanDirs [ buildDir; testResultsDir ]
)

Target "BuildMain" (fun _ ->
  !! "src/DocPlagiarizer/**/*.csproj"
    |> MSBuildRelease buildDir "Build"
    |> Log "AppBuild-Output: "
)

Target "PrepareTestProject" (fun _ ->
  !! "src/DocPlagiarizer.TestProject/originals/*.cs"
    |> Copy "src/DocPlagiarizer.TestProject/"
)

Target "TestProject" (fun _ ->
  !! "src/DocPlagiarizer.TestProject/**/*.csproj"
    |> MSBuildRelease "testproject" "Build"
    |> Log "AppBuild-Output: "
)

Target "ConfirmTests" (fun _ ->
  let builtFile (expectedFile: string) =
    expectedFile.Replace("expected", "")

  let ConfirmFilesAreEqual (first, second) =
    match (FilesAreEqual first second) with
    | false -> logToConsole("Copying comments in `" + first.Name + "` failed!", System.Diagnostics.EventLogEntryType.Error)
    | true -> logToConsole("Copying comments in `" + first.Name + "` was successful.", System.Diagnostics.EventLogEntryType.Information)

  !! "src/DocPlagiarizer.TestProject/expected/*.cs"
    |> Seq.map (fun expected -> (expected, (builtFile expected)))
    |> Seq.map (fun (expected, actual) -> (fileInfo expected, fileInfo actual))
    |> Seq.iter ConfirmFilesAreEqual
)

Target "Default" DoNothing

// Dependencies
"Clean"
  ==> "BuildMain"
  ==> "PrepareTestProject"
  ==> "TestProject"
  ==> "ConfirmTests"
  ==> "Default"

// Start
RunTargetOrDefault "Default"
