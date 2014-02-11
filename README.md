#DocPlagiarizer

##What is it?

A build task for MSBuild that will copy xml documentation comments from interfaces to implementations.

##How do I use it?!

- Download and build the source.
- Drop the resulting dll's into a folder in your project.
- Add the following line to your project file. `<UsingTask TaskName="DocPlagiarizerTask" AssemblyFile="<PathToPlagiarizerFolder>\DocPlagiarizer.dll" />`
- Add the task to the BeforeBuild target
    &lt;Target Name="BeforeBuild"&gt;
        &lt;DocPlagiarizerTask /&gt;
    &lt;/Target&gt;