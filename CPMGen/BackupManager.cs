namespace CPMGen;

public class BackupManager
{
    public string? CreateBackupDirectory(Options opt)
    {
        string backupPath = null;
        if (!opt.NoBackup)
        {
            backupPath = Path.GetFullPath(string.IsNullOrEmpty(opt.BackupDir) ? "." : opt.BackupDir);
            Directory.CreateDirectory(backupPath);
            Console.WriteLine($"Backup directory: {backupPath}");
        }

        return backupPath;
    }
    
    public void CreateBackupForProject(Options opt, string projectFilePath, string backupPath)
    {
        if (!opt.NoBackup)
        {
            var backupFileName = $"{Path.GetFileName(projectFilePath)}.backup_{DateTime.Now:yyyyMMddHHmmss}";
            var backupFile = Path.Combine(backupPath!, backupFileName);
            File.Copy(projectFilePath, backupFile);
            Console.WriteLine($"Backed up: {backupFileName} to {backupFile}");
        }
    }
    
    public async Task ManageGitIgnore(Options options, string? s)
    {
        if (options is { AddBackupToGitignore: true, NoBackup: false })
        {
            
            var gitignorePath = Path.GetFullPath(options.GitignoreDir);
            var gitignoreFile = Path.Combine(gitignorePath, ".gitignore");

            var backupRelativePath = Path.GetRelativePath(gitignorePath, s!);
            var gitignoreEntry = $"\n# CPMGen backup directory\n{backupRelativePath}/\n";

            if (File.Exists(gitignoreFile))
            {
                var content = await File.ReadAllTextAsync(gitignoreFile);
                if (!content.Contains(backupRelativePath))
                {
                    await File.AppendAllTextAsync(gitignoreFile, gitignoreEntry);
                    Console.WriteLine($"Added backup directory to existing .gitignore");
                }
            }
            else
            {
                await File.WriteAllTextAsync(gitignoreFile, gitignoreEntry);
                Console.WriteLine($"Created .gitignore with backup directory");
            }
        }
    }
}