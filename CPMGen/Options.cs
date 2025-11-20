using System;
using CommandLine;

namespace CPMGen;
/*
 * options:
 *  --no-backup: disables the default backup option
 *  --keep-attrs -k : doesn't remove the version attribute from the .csproj files
 *  --solution -s : sepcifies the solution file
 *  --project -p : specifies the project file
 *  --output -o : the props file location
 */

public class Options
{
    [Option('n', "no-backup", Default = false, 
        HelpText = "Disables the default backup option", Required = false) ]
    public bool NoBackup { get; set; }

    [Option('k', "keep-attrs", Default = false, 
        HelpText = "Keeps the 'Version' attribute in the .csproj files.")]
    public bool KeepAttributes { get; set; }

    [Option('s', "solution", 
        HelpText = "Specifies the solution file location. If this option is provided " +
                   "the project file location option will be ignored.", Required = false)]
    public string SolutionFileDir { get; set; } = string.Empty;

    [Option('p', "project", HelpText = "Specifies the project file location. ")]
    public string ProjectFileDir { get; set; } = string.Empty;

    [Option('o', "output-dir", HelpText = "The props file output directory.", Default = ".")]
    public string OutputDir { get; set; } = string.Empty;
}