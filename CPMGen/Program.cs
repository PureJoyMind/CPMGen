// See https://aka.ms/new-console-template for more information

using System.Text.RegularExpressions;
using Buildalyzer;
using Buildalyzer.Construction;

var basePath = @"E:\VS Projects\CPMGen";

    var solutionPath = Path.Combine(basePath, "CPMGen.sln");   
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

        Console.WriteLine($"Project Name: {projectName}, Project Path: {projectPath}");
        var manager = new AnalyzerManager();
        var analyzer = manager.GetProject(Path.Combine(basePath, projectPath));
        foreach (var reference in analyzer.ProjectFile.PackageReferences)
        {
            if(packages.TryGetValue(reference.Name, out var value)) value.Add(reference.Version);
        }
        
        
    }
    
}
else
{
    Console.WriteLine("Solution file not found.");
}