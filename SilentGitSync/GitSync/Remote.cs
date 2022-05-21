namespace SilentOrbit.GitSync;

/// <summary>
/// Remote name and base path
/// </summary>
class Remote
{
    public readonly SyncConfig SyncConfig;
    public string Name { get; set; }
    public Repo Git { get; set; }

    public Remote(SyncConfig config, string remoteName, string remotePath)
    {
        this.SyncConfig = config;
        this.Name = remoteName;
        if (remotePath == null)
            remotePath = config.Remote[Name];
        this.Git = new Repo(remotePath);
    }

    public override string ToString()
    {
        return Name + ": " + Git.Path;
    }

    /// <summary>
    /// Greate new remote
    /// </summary>
    /// <param name="subPath"></param>
    /// <returns></returns>
    public Remote Append(string subPath)
    {
        var path = Git.Path;
        if (path.Contains("/"))
            path = path.TrimEnd('/') + "/" + subPath.Replace('\\', '/').TrimStart('/');
        else
            path = path.TrimEnd('\\') + "\\" + subPath.Replace("/", "\\").TrimStart('\\');

        return new Remote(SyncConfig, Name, path);
    }
}
