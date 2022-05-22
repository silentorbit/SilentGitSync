using SilentOrbit;
using SilentOrbit.GitSync;

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



//Load config from JSON file
//var scanner = ConfigLoader.LoadConfig("GitSync.json");
//scanner.RunAllRemotes();
