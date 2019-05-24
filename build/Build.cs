using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.MSBuild;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;

[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main () => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("ApiKey for the specified source.")] 
    readonly string ApiKey;

    [Solution] 
    readonly Solution Solution;

    [GitRepository] 
    readonly GitRepository GitRepository;

    [GitVersion] 
    readonly GitVersion GitVersion;

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath ProjectDirectory => SourceDirectory / "RouteServiceIwaWcfInterceptor";
    AbsolutePath ProjectFile => ProjectDirectory / "RouteServiceIwaWcfInterceptor.csproj";
    AbsolutePath TestsDirectory => RootDirectory / "tests";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    
    string Branch => GitRepository.Branch;
    string ChangelogFile => RootDirectory / "CHANGELOG.md";

    string Source => @"c:\MyLocalNugetRepo";

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            TestsDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            EnsureCleanDirectory(ArtifactsDirectory);
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            MSBuild(s => s
                .SetTargetPath(Solution)
                .SetTargets("Restore"));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            MSBuild(s => s
                .SetTargetPath(Solution)
                .SetTargets("Rebuild")
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(GitVersion.GetNormalizedAssemblyVersion())
                .SetFileVersion(GitVersion.GetNormalizedFileVersion())
                .SetInformationalVersion(GitVersion.InformationalVersion)
                .SetMaxCpuCount(Environment.ProcessorCount)
                .SetNodeReuse(IsLocalBuild));
        });

    Target Pack => _ => _
    .DependsOn(Compile)
    .Executes(() =>
    {
        var changelogUrl = GitRepository.GetGitHubBrowseUrl(ChangelogFile, branch: "master");

        DotNetTasks.DotNetPack(s => s
            .SetPackageReleaseNotes(changelogUrl)
            .SetWorkingDirectory(ProjectDirectory)
            .SetProject(ProjectFile)
            .EnableNoBuild()
            .SetConfiguration(Configuration)
            .EnableIncludeSymbols()
            .SetOutputDirectory(ArtifactsDirectory)
            .SetVersion(GitVersion.NuGetVersionV2));
    });

    // Target Push => _ => _
    // .DependsOn(Pack)
    // //.Requires(() => ApiKey)
    // .Requires(() => Source)
    // .Executes(() =>
    // {
    //     GlobFiles(ArtifactsDirectory, "*.nupkg").NotEmpty()
    //         .Where(x => !x.EndsWith(".symbols.nupkg"))
    //         .ForEach(x => DotNetNuGetPush(s => s
    //             .SetTargetPath(x)
    //             .SetSource(Source)
    //            // .SetApiKey(ApiKey)
    //             ));
    // });
}