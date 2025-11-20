// See https://aka.ms/new-console-template for more information

using System.Text;
using System.Text.RegularExpressions;
using Buildalyzer;
using Buildalyzer.Construction;

/*
 * options:
 *  --no-backup: disables the default backup option
 *  --solution -s : sepcifies the solution file
 *  --project -p : specifies the project file
 *  --no-remove : doesn't remove the version attribute from the .csproj files
 */


var basePath = @"E:\tmp\Mentorly - Copy (2)";

    var solutionPath = Path.Combine(basePath, "Mentorly.sln");   
if (File.Exists(solutionPath))
{
    var solutionContent = File.ReadAllText(solutionPath);

    var matches = Regex.Matches(solutionContent, 
        @"Project\(""\{(.+?)\}""\) = ""(.+?)"", ""(.+?)""", RegexOptions.Multiline);

    
    var packages = new Dictionary<string, HashSet<string>>();
    
    foreach (Match match in matches)
    {
        var projectId = match.Groups[1].Value;
        var projectName = match.Groups[2].Value;
        var projectPath = match.Groups[3].Value;
        if(!projectPath.EndsWith("csproj")) continue;
        Console.WriteLine($"Project Name: {projectName}, Project Path: {projectPath}");
        var manager = new AnalyzerManager();
        var projectFilePath = Path.Combine(basePath, projectPath);
        var analyzer = manager.GetProject(projectFilePath);
        
        var projectFileContent = new StringBuilder(File.ReadAllText(projectFilePath));
        
        foreach (var reference in analyzer.ProjectFile.PackageReferences)
        {
            if(packages.TryGetValue(reference.Name, out var value)) value.Add(reference.Version);
            else
                packages.Add(reference.Name, new HashSet<string> { reference.Version });

            projectFileContent.Replace($"Version=\"{reference.Version}\"", "");
        }
        
        // TODO: you have the replaced file content, write it to .csproj file
        await File.WriteAllTextAsync(projectFilePath, projectFileContent.ToString());
    }


    var str = """
                       <Project>
                         <PropertyGroup>
                           <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
                         </PropertyGroup>
                         <ItemGroup>
                       """;

    var packageProps = new StringBuilder();
    packageProps.AppendLine(str);
    
    foreach (var kvp in packages)
    {
        foreach (var version in kvp.Value)
            packageProps.AppendLine($"""    <PackageVersion Include="{kvp.Key}" Version="{version}" />""");
    }

    packageProps.AppendLine("""
                              </ItemGroup>
                            </Project>
                            """);

    await File.WriteAllTextAsync(Path.Combine(basePath, "Directory.Packages.props"), packageProps.ToString());
}
else
{
    Console.WriteLine("Solution file not found.");
}