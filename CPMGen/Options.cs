using System.Text;
using System.Text.RegularExpressions;
using Buildalyzer;
using CommandLine;
using CommandLine.Text;

namespace CPMGen;

public partial class Options
{
    #region options
    
    [Option('s', "solution",
        HelpText = "Specifies the solution file location. If this option is provided " +
                   "the project file location option will be ignored.", Required = false, Default = ".")]
    public string SolutionFileDir { get; set; } = string.Empty;

    [Option('p', "project", HelpText = "Specifies the project file location. ")]
    public string ProjectFileDir { get; set; } = string.Empty;

    [Option('o', "output-dir", HelpText = "The props file output directory.", Default = ".")]
    public string OutputDir { get; set; } = string.Empty;

    [Option('k', "keep-attrs", Default = false,
        HelpText = "Keeps the 'Version' attribute in the .csproj files.")]
    public bool KeepAttributes { get; set; }


    [Option('n', "no-backup", Default = false,
        HelpText = "Disables the default backup option", Required = false)]
    public bool NoBackup { get; set; }

    [Option("backup-dir", Default = ".",
        HelpText = "The backup directory for .csproj files about to be changed.")]
    public string BackupDir { get; set; }

    [Option("add-gitignore", Default = false,
        HelpText = "Adds the backup directory to .gitignore file. Creates one if not present")]
    public bool AddBackupToGitignore { get; set; }

    [Option("gitignore-dir", Default = ".",
        HelpText = "The directory for .gitignore file if there isn't one existing.")]
    public string GitignoreDir { get; set; }
    
    [Usage(ApplicationAlias = "CPMGen")]
    public static IEnumerable<Example> Examples =>
        new List<Example>()
        {
            new("Default behaviour", new Options { }),
            new("Convert only one project",
                new Options { ProjectFileDir = Path.Combine("path", "to", "project.csproj") }),
            new("Specify the output directory, generates '../upDir/Directory.Packages.props'",
                new Options { OutputDir = Path.Combine("..", "upDir") }),
        };

    #endregion


    #region Methods

    public void Validate()
    {
        if (NoBackup && AddBackupToGitignore)
        {
            Console.Error.WriteLine($"Error: --add-gitignore can only be used when --no-backup is true.");
            Environment.Exit(1);
        }

        if (!NoBackup && string.IsNullOrWhiteSpace(BackupDir))
        {
            Console.Error.WriteLine("Error: backup-dir must be specified when backup is enabled.");
            return;
        }

        if (AddBackupToGitignore && string.IsNullOrWhiteSpace(GitignoreDir))
        {
            Console.Error.WriteLine("Error: gitignore-dir must be specified when add-gitignore is enabled.");
            return;
        }

        if (!string.IsNullOrEmpty(SolutionFileDir) && !string.IsNullOrWhiteSpace(ProjectFileDir))
            Console.WriteLine("Both solution and project directories are included, will use solution file as source." +
                              "\r\nWill ignore the project file specified.");
    }

    public void HandleSolution(ref string basePath, List<string> projectPaths)
    {
        var solutionPath = Path.GetFullPath(SolutionFileDir);

        if (Directory.Exists(solutionPath))
        {
            // Look for .sln file in directory
            var slnFiles = Directory.GetFiles(solutionPath, "*.sln");
            if (slnFiles.Length == 0)
            {
                Console.WriteLine("No solution file found in the specified directory.");
                return;
            }

            solutionPath = slnFiles[0];
        }

        if (!File.Exists(solutionPath))
        {
            Console.WriteLine("Solution file not found.");
            return;
        }

        basePath = Path.GetDirectoryName(solutionPath)!;
        var solutionContent = File.ReadAllText(solutionPath);

        var matches = ProjectRegex().Matches(solutionContent);

        foreach (Match match in matches)
        {
            var projectPath = match.Groups[3].Value;
            if (!projectPath.EndsWith("csproj")) continue;

            var projectFilePath = Path.GetFullPath(Path.Combine(basePath, projectPath));
            projectPaths.Add(projectFilePath);
            Console.WriteLine($"Found project: {match.Groups[2].Value}");
        }
    }


    public void HandleProject(ref string basePath, List<string> projectPaths)
    {
        var projectPath = Path.GetFullPath(ProjectFileDir);

        if (Directory.Exists(projectPath))
        {
            // Look for .csproj file in directory
            var projFiles = Directory.GetFiles(projectPath, "*.csproj");
            if (projFiles.Length == 0)
            {
                Console.WriteLine("No project file found in the specified directory.");
                return;
            }

            projectPath = projFiles[0];
        }

        if (!File.Exists(projectPath))
        {
            Console.WriteLine("Project file not found.");
            return;
        }

        projectPaths.Add(projectPath);
        basePath = Path.GetDirectoryName(projectPath)!;
    }

    public string ProcessProjectFileContent(AnalyzerManager analyzerManager, string s,
        Dictionary<string, HashSet<string>> dictionary,
        Options options)
    {
        var analyzer = analyzerManager.GetProject(s);
        var stringBuilder = new StringBuilder(File.ReadAllText(s));

        foreach (var reference in analyzer.ProjectFile.PackageReferences)
        {
            if (dictionary.TryGetValue(reference.Name, out var value))
                value.Add(reference.Version);
            else
                dictionary.Add(reference.Name, new HashSet<string> { reference.Version });

            if (!options.KeepAttributes)
            {
                stringBuilder.Replace($"Version=\"{reference.Version}\"", "");
            }
        }

        return stringBuilder.ToString();
    }

    public string UpdatePackageProps(Dictionary<string, HashSet<string>> dictionary)
    {
        var s = """
                <Project>
                  <PropertyGroup>
                    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
                  </PropertyGroup>
                  <ItemGroup>
                """;
        
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine(s);

        foreach (var kvp in dictionary)
        {
            foreach (var version in kvp.Value)
                stringBuilder.AppendLine($"""    <PackageVersion Include="{kvp.Key}" Version="{version}" />""");
        }

        stringBuilder.AppendLine("""
                                   </ItemGroup>
                                 </Project>
                                 """);
        return stringBuilder.ToString();
    }

    #endregion

    /// <summary>
    /// Regex to find project files with their path and project name from .sln files.
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"Project\(""\{(.+?)\}""\) = ""(.+?)"", ""(.+?)""", RegexOptions.Multiline)]
    private static partial Regex ProjectRegex();
}