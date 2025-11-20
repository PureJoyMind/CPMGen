# CPMGen

A command-line tool that helps you quickly migrate large .NET solutions to Central Package Management (CPM). CPMGen automatically generates `Directory.Packages.props` files and updates your `.csproj` files, making the transition to centralized package management effortless.

## Why CPMGen?

Migrating large solutions with multiple projects to Central Package Management can be tedious and error-prone. CPMGen automates this process, allowing you to modernize your .NET solution's package management in seconds instead of hours.

## Features

- 🔍 Automatically scans for `.sln` or `.csproj` files in your solution, or you can tell it where to look
- 📦 Generates `Directory.Packages.props` with all package versions centralized
- 🔄 Updates project files to remove version attributes automatically
- 💾 Built-in backup functionality to keep your original files safe
- 🚫 Optional `.gitignore` integration for backup directories

## Installation

```bash
# Install as a global .NET tool
dotnet tool install --global CPMGen
```

After installation, the `CPMGen` command will be available globally in your terminal.

## Usage

### Basic Usage

```bash
# Convert all projects in current directory
CPMGen

# Convert projects in a specific solution
CPMGen -s path/to/solution

# Convert a specific project
CPMGen -p path/to/project.csproj
```

### Options

| Option            | Short | Description                                                     | Default                 |
|-------------------|-------|-----------------------------------------------------------------|-------------------------|
| `--solution`      | `-s`  | Directory to search for `.sln` files (overrides project option) | `.` (current directory) |
| `--project`       | `-p`  | Directory or path to `.csproj` file(s)                          | -                       |
| `--output-dir`    | `-o`  | Output directory for `Directory.Packages.props`                 | `.` (current directory) |
| `--keep-attrs`    | `-k`  | Keep `Version` attributes in `.csproj` files                    | `false`                 |
| `--no-backup`     | `-n`  | Disable automatic backup of `.csproj` files                     | `false`                 |
| `--backup-dir`    | -     | Directory for backing up modified `.csproj` files               | `.` (current directory) |
| `--add-gitignore` | -     | Add backup directory to `.gitignore` (creates if missing)       | `false`                 |
| `--gitignore-dir` | -     | Directory for `.gitignore`                                      | `.` (current directory) |

### Examples

```bash
# Default behavior - convert all projects in current directory
CPMGen

# Convert only a specific project
CPMGen -p path/to/project.csproj

# Specify custom output directory
CPMGen -o ../upDir

# Convert solution with custom backup location
CPMGen -s ./src --backup-dir ./backups --add-gitignore

# Convert without creating backups
CPMGen -n

# Keep version attributes in project files
CPMGen -k
```

## What is Central Package Management?

Central Package Management (CPM) is a NuGet feature that allows you to manage all your package versions in a single `Directory.Packages.props` file rather than scattered across individual project files. This provides:

- **Consistency**: Ensures all projects use the same package versions
- **Maintainability**: Update a package version in one place, apply everywhere
- **Reduced conflicts**: Fewer merge conflicts in project files during team collaboration
- **Better overview**: See all your dependencies and versions at a glance

### Learn More About CPM

- [Official Microsoft Documentation](https://learn.microsoft.com/en-us/nuget/consume-packages/central-package-management)
- [Central Package Management in .NET - Milan Jovanović](https://www.milanjovanovic.tech/blog/central-package-management-in-net-simplify-nuget-dependencies)

## How it works

On a standard execution:
1. You run `cpmgen`
2. Scans the current directory for `.sln` files, if multiple are found you can choose one
3. Parses all the projects defined in the `.sln` file
4. Backs up all the `.csproj` files it'll have to udpate so you won't lose any data
5. Generates the `Directory.Packages.props` file with all the packages + versions found in all  the projects
6. Updates the `.csproj` files to remove `Version` attributes from package references

## Requirements

- **.NET SDK**: Currently the tool is configured to be built for `.NET` versions 8 & 9, but I'll add other versions support soon
- **Project Format**: SDK-style `.csproj` files (the modern XML format)
- **Operating System**: Windows, macOS, or Linux

### Compatibility Note

CPMGen works with SDK-style projects introduced in 2017. It does **not** support the legacy `.csproj` format used in older .NET Framework projects. If you have a mixed solution, only SDK-style projects will be processed.

## Contributing

Contributions are welcome! Whether it's bug reports, feature requests, or code contributions, your input helps make CPMGen better for everyone.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the **GNU Affero General Public License v3.0 (AGPL-3.0)**.
This ensures CPMGen remains free and open-source for everyone, forever.
See the [LICENSE](LICENSE) file for details.

## Support

If you encounter any issues or have questions:
- 🐛 [Report a bug](https://github.com/purejoymind/cpmgen/issues)
- 💡 [Request a feature](https://github.com/purejoymind/cpmgen/issues)
- 💬 [Ask a question](https://github.com/purejoymind/cpmgen/discussions)

## Acknowledgments

Built to simplify the adoption of NuGet's Central Package Management feature in the .NET ecosystem.