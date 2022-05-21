namespace SilentOrbit.GitSync;

public class Scanner
{
    readonly SyncConfig syncConfig;

    public Scanner(SyncConfig config)
    {
        this.syncConfig = config;

        config.Validate();
    }

    public void RunAllRemotes()
    {
        foreach (var remote in syncConfig.Remote)
            RunRemote(remote);
    }

    public void RunRemote(string remoteName)
    {
        var remote = syncConfig.Remote.First(r => r.Name == remoteName);
        RunRemote(remote);
    }

    public void RunRemote(RemoteConfig remote)
    {
        foreach (var repo in syncConfig.Repo)
        {
            RunRepo(repo, remote);
        }
    }

    void RunRepo(RepoConfig config, RemoteConfig remoteConfig)
    {
        if (config.Remote.Contains(remoteConfig.Name) == false)
            return;

        if (config.RecursiveOnly)
        {
            Scan((DirPath)config.Path, remoteConfig, skipRoot: true);
        }
        else if (config.Recursive)
        {
            Scan((DirPath)config.Path, remoteConfig);
        }
        else
        {
            //Single repo
            var gitRepo = new Repo(config.Path);
            if (gitRepo.IsWorkTree)
                return; //Skip worktrees as their main repo will be taken care of

            Remote remote = remoteConfig.GetRemote(gitRepo);
            RepoSync.SyncRepo(gitRepo, remote, syncConfig);
        }
    }

    void Scan(DirPath sourceBase, RemoteConfig remoteConfig, bool skipRoot = false)
    {
        foreach (var gitPath in ScanGit(sourceBase))
        {
            if (skipRoot)
            {
                if (gitPath.Parent == sourceBase)
                    continue;
            }

            var repoPath = gitPath.Parent;
            var source = new Repo(repoPath.Path, (repoPath - sourceBase).RelativePath);
            if (source.IsWorkTree)
                continue; //Skip worktrees as their main repo will be taken care of

            var target = remoteConfig.GetRemote(source);
            RepoSync.SyncRepo(source, target, syncConfig);
        }
    }

    static IEnumerable<FullDiskPath> ScanGit(DirPath root)
    {
        var list = new List<FullDiskPath>();

        foreach (var path in root.GetDirectories(".git", SearchOption.AllDirectories))
            list.Add(path);

        foreach (var path in root.GetFiles(".git", SearchOption.AllDirectories))
            list.Add(path);

        //Reverese sort order to make sure sub repos are synchronized before the main repos are
        list.Sort();
        list.Reverse();
        return list;
    }
}
