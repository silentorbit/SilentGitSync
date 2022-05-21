namespace SilentOrbit.GitSync;

public partial class SyncConfig
{
    /// <summary>
    /// Key: git remote name
    /// Value: remote path/url
    /// </summary>
    public List<RemoteConfig> Remote { get; set; } = new List<RemoteConfig>();

    /// <summary>
    /// Scan these paths and subdirectories for any git repo.
    /// </summary>
    public List<RepoConfig> Repo { get; set; } = new List<RepoConfig>();

    /// <summary>
    /// Fetch and merge before push
    /// </summary>
    public bool FetchBeforePush { get; set; }

    /// <summary>
    /// Check configuration for mistakes
    /// </summary>
    public void Validate()
    {
        //Make sure all repo remotes exists in Remote.
        foreach (var r in Repo)
        {
            foreach (var remote in r.Remote)
            {
                if (Remote.Any(r => r.Name == remote))
                    continue;
                else
                    throw new ArgumentException("Remote " + remote + " references in " + r.Path + " does not exist in list of remotes");
            }
        }
    }

    public RemoteConfig AddRemote(string name, string url)
    {
        var remote = new RemoteConfig { Name = name, Path = url };
        this.Remote.Add(remote);
        return remote;
    }

}
