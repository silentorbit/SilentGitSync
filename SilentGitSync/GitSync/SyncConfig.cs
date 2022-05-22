namespace SilentOrbit.GitSync;

public class SyncConfig
{
    /// <summary>
    /// Scan these paths and subdirectories for any git repo.
    /// </summary>
    public List<RepoConfig> Repo { get; set; } = new List<RepoConfig>();

    /// <summary>
    /// Push to these remote repos.
    /// Can be local paths, for example an external drive, or remote git/ssh paths.
    /// </summary>
    public List<RemoteConfig> Remote { get; set; } = new List<RemoteConfig>();

    /// <summary>
    /// Fetch and merge before push
    /// </summary>
    public bool FetchBeforePush { get; set; }

    public RemoteConfig AddRemote(string name, string url)
    {
        var remote = new RemoteConfig { Name = name, Path = url };
        Remote.Add(remote);
        return remote;
    }

}
