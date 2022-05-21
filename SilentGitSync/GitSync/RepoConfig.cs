namespace SilentOrbit.GitSync;

/// <summary>
/// Local repo to sync
/// </summary>
public class RepoConfig
{
    /// <summary>
    /// Path to local
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    /// Scan recursively, including path
    /// </summary>
    public bool Recursive { get; set; }

    /// <summary>
    /// Scan recursively, excluding path
    /// </summary>
    public bool RecursiveOnly { get; set; }

    /// <summary>
    /// Remote names
    /// </summary>
    public List<string> Remote { get; set; } = new List<string>();

    public override string ToString()
    {
        return Path + " [" + (Recursive ? "recursive" : "") + (RecursiveOnly ? "recursiveOnly" : "") + "] " + string.Join(", ", Remote);
    }
}
