using Buildalyzer;
using CommandLine;
using CPMGen;

await Parser.Default.ParseArguments<Options>(args)
    .WithParsedAsync(async opt =>
    {
        opt.Validate();
        
        var basePath = "";
        var projectPaths = new List<string>();

        if (!string.IsNullOrWhiteSpace(opt.SolutionFileDir))
        {
           opt.HandleSolution(ref basePath, projectPaths); 
        }
        else if (!string.IsNullOrWhiteSpace(opt.ProjectFileDir))
        {
            opt.HandleProject(ref basePath, projectPaths);
        }
        else
        {
            Console.WriteLine("Either solution or project path must be specified.");
            return;
        }

        var packages = new Dictionary<string, HashSet<string>>();
        var manager = new AnalyzerManager();
        var backupManager = new BackupManager();
        
        var backupPath =  backupManager.CreateBackupDirectory(opt);
        
        foreach (var projectFilePath in projectPaths)
        {
            backupManager.CreateBackupForProject(opt, projectFilePath, backupPath!);

            var projectFileContent = opt.ProcessProjectFileContent(manager, projectFilePath, packages, opt);

            await File.WriteAllTextAsync(projectFilePath, projectFileContent);
            Console.WriteLine($"Updated: {Path.GetFileName(projectFilePath)}");
        }

        var updatedPackagePropsContent = opt.UpdatePackageProps(packages);

        var outputPath = Path.GetFullPath(opt.OutputDir);
        var propsFilePath = Path.Combine(outputPath, "Directory.Packages.props");
        await File.WriteAllTextAsync(propsFilePath, updatedPackagePropsContent);
        Console.WriteLine($"Generated: {propsFilePath}");

        // Add to .gitignore if requested
        await backupManager.ManageGitIgnore(opt, backupPath);

        Console.WriteLine("\nCompleted successfully!");
    });