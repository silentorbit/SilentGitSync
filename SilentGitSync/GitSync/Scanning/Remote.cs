namespace SilentOrbit.GitSync.Scanning;

/// <summary>
/// Remote name and remote git repo
/// </summary>
class Remote
{
    public string Name { get; set; }
    public Repo Git { get; set; }

    public Remote(string remoteName, string remotePath)
    {
        Name = remoteName ?? throw new ArgumentNullException();
        Git = new Repo(remotePath ?? throw new ArgumentNullException());
    }

    public override string ToString()
    {
        return Name + ": " + Git.Path;
    }
}
