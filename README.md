#DocPlagiarizer

##What is it?

A build task for MSBuild that will copy xml documentation comments from interfaces to implementations.

##How do I use it?!

- Download and build the source by running `build.bat`.
- Copy the built libraries from `./build/` into a folder in your project.
- Add the following line to your project file.
`<UsingTask TaskName="DocPlagiarizerTask" AssemblyFile="<PathToPlagiarizerFolder>\DocPlagiarizer.dll" />`
- Add the task to the `BeforeBuild` target in the same project file
````
    <Target Name="BeforeBuild">
        <DocPlagiarizerTask />;
    </Target>;
````