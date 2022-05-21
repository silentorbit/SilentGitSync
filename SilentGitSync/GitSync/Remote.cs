namespace SilentOrbit.GitSync;

/// <summary>
/// Remote name and base path
/// </summary>
class Remote
{
    public string Name { get; set; }
    public Repo Git { get; set; }

    public Remote(string remoteName, string remotePath)
    {
        this.Name = remoteName ?? throw new ArgumentNullException();
        this.Git = new Repo(remotePath ?? throw new ArgumentNullException());
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

        return new Remote(Name, path);
    }
}
