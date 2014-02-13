#r "tools/FAKE/tools/FakeLib.dll"
open Fake
open System
open System.Diagnostics

RestorePackages()

// Properties
let version = "0.1.0"
let buildDir = "./build/"
let testResultsDir = "./testresults/"
let distDir = "./dist/"
let distToolsDir = distDir @@ "tools/"
let distContentDir = distDir @@ "content/"
let buildMode = getBuildParamOrDefault "buildMode" "Release"

// Targets
Target "Clean" (fun _ ->
  CleanDirs [ buildDir
              testResultsDir
              distDir
              distToolsDir
              distContentDir ]
)

Target "Build" (fun _ ->
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
    | false -> logToConsole("Copying comments in `" + first.Name + "` failed!", EventLogEntryType.Error)
    | true -> logToConsole("Copying comments in `" + first.Name + "` was successful.", EventLogEntryType.Information)

  !! "src/DocPlagiarizer.TestProject/expected/*.cs"
    |> Seq.map (fun expected -> (expected, (builtFile expected)))
    |> Seq.map (fun (expected, actual) -> (fileInfo expected, fileInfo actual))
    |> Seq.iter ConfirmFilesAreEqual
)

Target "Package" (fun _ ->
  !! (buildDir @@ "*.dll")
  ++ "uninstall.ps1"
  ++ "install.ps1"
    |> Copy distToolsDir
  "README.md"
    |> CopyFile (distContentDir @@ "DocPlagiarizer.README.md")

  NuGet (fun p ->
    { p with
        WorkingDir = distDir
        OutputPath = distDir
        Publish = false
        Version = version }) "DocPlagiarizer.nuspec"
)

Target "Default" DoNothing

// Dependencies
"Clean"
  ==> "Build"
  ==> "PrepareTestProject"
  ==> "TestProject"
  ==> "ConfirmTests"
  ==> "Default"
  ==> "Package"

// Start
RunTargetOrDefault "Default"
