// See https://aka.ms/new-console-template for more information

using System.Text;
using System.Text.RegularExpressions;
using Buildalyzer;
using Buildalyzer.Construction;

var basePath = @"E:\VS Projects\Mentorly";

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
        var analyzer = manager.GetProject(Path.Combine(basePath, projectPath));
        foreach (var reference in analyzer.ProjectFile.PackageReferences)
        {
            if(packages.TryGetValue(reference.Name, out var value)) value.Add(reference.Version);
            else
                packages.Add(reference.Name, new HashSet<string> { reference.Version });
        }
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
    
    await using var fs = File.Create(Path.Combine(basePath, "Directory.Packages.props"));
    await using var writer = new StreamWriter(fs);
    await writer.WriteAsync(packageProps.ToString());
    // await fs.WriteAsync;
}
else
{
    Console.WriteLine("Solution file not found.");
}