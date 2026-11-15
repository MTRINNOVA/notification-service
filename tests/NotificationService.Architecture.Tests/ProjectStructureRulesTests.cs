using System.Xml.Linq;

namespace NotificationService.Architecture.Tests;

/// <summary>
/// Reglas sobre los archivos de proyecto: dirección de referencias, framework y gestión centralizada de paquetes.
/// Son efectivas aunque los ensamblados aún no contengan tipos que NetArchTest pueda inspeccionar.
/// </summary>
public sealed class ProjectStructureRulesTests
{
    private static readonly string RepositoryRoot = FindRepositoryRoot();

    public static TheoryData<string, string[]> AllowedProjectReferences => new()
    {
        { "NotificationService.Domain", [] },
        { "NotificationService.Application", ["NotificationService.Domain"] },
        { "NotificationService.Infrastructure", ["NotificationService.Application", "NotificationService.Domain"] },
        { "NotificationService.Api", ["NotificationService.Application", "NotificationService.Infrastructure"] },
        { "NotificationService.Worker", ["NotificationService.Application", "NotificationService.Infrastructure"] },
        { "NotificationService.Scheduler", ["NotificationService.Application", "NotificationService.Infrastructure"] }
    };

    [Theory]
    [MemberData(nameof(AllowedProjectReferences))]
    public void SourceProjects_ReferenceOnlyAllowedProjects(string project, string[] allowed)
    {
        var references = LoadProject(Path.Combine("src", project, project + ".csproj"))
            .Descendants("ProjectReference")
            .Select(reference => Path.GetFileNameWithoutExtension(reference.Attribute("Include")!.Value.Replace('\\', '/')))
            .ToList();

        var forbidden = references.Except(allowed, StringComparer.Ordinal).ToList();

        Assert.True(forbidden.Count == 0, $"{project} must not reference: {string.Join(", ", forbidden)}");
    }

    [Fact]
    public void Domain_HasNoPackageReferences()
    {
        var packages = PackageReferences(Path.Combine("src", "NotificationService.Domain", "NotificationService.Domain.csproj"));

        Assert.Empty(packages);
    }

    [Fact]
    public void Application_ReferencesOnlyAbstractionPackages()
    {
        var packages = PackageReferences(
            Path.Combine("src", "NotificationService.Application", "NotificationService.Application.csproj"));

        Assert.All(packages, package => Assert.EndsWith(".Abstractions", package, StringComparison.Ordinal));
    }

    [Fact]
    public void AllProjects_TargetNet10()
    {
        foreach (var projectFile in AllProjectFiles())
        {
            var targetFramework = XDocument.Load(projectFile).Descendants("TargetFramework").SingleOrDefault()?.Value;

            Assert.True(targetFramework == "net10.0", $"{Path.GetFileName(projectFile)} targets '{targetFramework}'.");
        }
    }

    [Fact]
    public void AllProjects_UseCentralPackageVersions()
    {
        foreach (var projectFile in AllProjectFiles())
        {
            var inlineVersions = XDocument.Load(projectFile)
                .Descendants("PackageReference")
                .Where(reference => reference.Attribute("Version") is not null || reference.Element("Version") is not null)
                .Select(reference => reference.Attribute("Include")?.Value)
                .ToList();

            Assert.True(
                inlineVersions.Count == 0,
                $"{Path.GetFileName(projectFile)} declares inline versions for: {string.Join(", ", inlineVersions)}");
        }
    }

    [Fact]
    public void CentralPackageVersions_AreStable()
    {
        var prerelease = CentralPackageVersions()
            .Where(package => package.Version.Contains('-', StringComparison.Ordinal))
            .Select(package => $"{package.Id} {package.Version}")
            .ToList();

        Assert.True(prerelease.Count == 0, $"Prerelease packages are not allowed: {string.Join(", ", prerelease)}");
    }

    [Fact]
    public void EntityFrameworkCorePackages_ShareMajorAndMinorVersion()
    {
        var efCoreVersions = CentralPackageVersions()
            .Where(package => package.Id.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal))
            .Select(package => string.Join('.', package.Version.Split('.').Take(2)))
            .Distinct()
            .ToList();

        Assert.Single(efCoreVersions);
    }

    [Fact]
    public void NpgsqlProvider_SharesMajorVersionWithEntityFrameworkCore()
    {
        var versions = CentralPackageVersions().ToDictionary(package => package.Id, package => package.Version);

        var efCoreMajor = versions["Microsoft.EntityFrameworkCore"].Split('.')[0];
        var npgsqlMajor = versions["Npgsql.EntityFrameworkCore.PostgreSQL"].Split('.')[0];

        Assert.Equal(efCoreMajor, npgsqlMajor);
    }

    private static IEnumerable<string> AllProjectFiles() =>
        new[] { "src", "tests" }
            .SelectMany(folder => Directory.EnumerateFiles(Path.Combine(RepositoryRoot, folder), "*.csproj", SearchOption.AllDirectories))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal));

    private static List<string> PackageReferences(string relativeProjectPath) =>
        LoadProject(relativeProjectPath)
            .Descendants("PackageReference")
            .Select(reference => reference.Attribute("Include")!.Value)
            .ToList();

    private static IEnumerable<(string Id, string Version)> CentralPackageVersions() =>
        XDocument.Load(Path.Combine(RepositoryRoot, "Directory.Packages.props"))
            .Descendants("PackageVersion")
            .Select(package => (package.Attribute("Include")!.Value, package.Attribute("Version")!.Value));

    private static XDocument LoadProject(string relativePath) => XDocument.Load(Path.Combine(RepositoryRoot, relativePath));

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "NotificationService.sln")))
            {
                return directory.FullName;
            }
        }

        throw new InvalidOperationException("NotificationService.sln was not found above the test output directory.");
    }
}
