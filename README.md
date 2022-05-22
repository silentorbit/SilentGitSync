# GitSync

Makes sure all your git repos are fully pushed to the right location

 - Scan for all git repos
 - Add/update remote paths
 - Push new changes
 
# Usage

[Install NuGet package: SilentOrbit.SilentGitSync](https://www.nuget.org/packages/SilentOrbit.SilentGitSync/)

```C#
//Config in code
var config = new SyncConfig();

//Source repos
config.Repo.Add(new RepoConfig
{
    Path = @"C:\MyRepos",
    Remote = new List<string> { "github", "external" }
});

//Target repo
config.AddRemote("external", @"E:\Git\");

//Target repo with custom remote for some repos
var github = config.AddRemote("github", @"git@github.com:myaccount/");
github.AddAlias("SilentDisk", "git@github.com:silentorbit/SilentDisk.git");
github.AddAlias("SilentGitSync", "git@github.com:silentorbit/SilentGitSync.git");

//Start the sync
var scanner = new Scanner(config);
scanner.RunAllRemotes();
```
 
 