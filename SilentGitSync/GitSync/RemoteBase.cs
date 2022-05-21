
namespace SilentOrbit.GitSync;

class RemoteBase
{
    public readonly SyncConfig SyncConfig;
    public string Name { get; set; }
    readonly string RemotePath;

    public RemoteBase(SyncConfig config, string remoteName)
    {
        this.SyncConfig = config;
        this.Name = remoteName;
        RemotePath = config.Remote[Name];
    }

    public override string ToString() => Name + ": " + RemotePath;

    public Remote GenerateRemote(Repo repo)
    {
        var path = GenerateRemoteFlatGit(repo.Path, out var name);
        if (SyncConfig.RemoteAlias.TryGetValue(path, out var newPath))
        {
            if (string.IsNullOrWhiteSpace(newPath))
                return null;
            path = newPath;
        }
        if (SyncConfig.RemoteAlias.TryGetValue(name, out newPath))
        {
            if (string.IsNullOrWhiteSpace(newPath))
                return null;

            throw new NotImplementedException("Can only block by name");
        }
        return new Remote(SyncConfig, Name, path);
    }

    string GenerateRemoteFlatGit(string sourceRepo, out string name)
    {
        name = Path.GetFileName(sourceRepo);

        //Make sure remote for bare repos doesn't get double .git extension
        if (name.EndsWith(".git"))
            name = name.Substring(0, name.Length - ".git".Length);

        if (RemotePath.StartsWith("ssh://", StringComparison.OrdinalIgnoreCase))
            return RemotePath.TrimEnd('/') + "/" + name + ".git";
        else
            return Path.Combine(RemotePath, name + ".git");
    }

}
