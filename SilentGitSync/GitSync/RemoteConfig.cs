using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilentOrbit.GitSync;

public class RemoteConfig
{
    /// <summary>
    /// Git remote name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Git remote path:
    /// - local path
    /// - ssh
    /// - git
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    /// Map generated remote url to custom url.
    /// </summary>
    public Dictionary<string, string> RemoteAlias { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name">Repo base name without .git</param>
    /// <param name="path">new remote path</param>
    public void AddAlias(string name, string path)
    {
        RemoteAlias.Add(name, path);
    }

    internal Remote GetRemote(Repo repo)
    {
        var name = System.IO.Path.GetFileName(repo.Path);

        var path = GenerateRemoteFlatGit(name) ?? throw new NullReferenceException();
        if (RemoteAlias.TryGetValue(path, out var newpath))
        {
            if (string.IsNullOrWhiteSpace(newpath))
                return null; //Don't push
            path = newpath;
        }
        if (RemoteAlias.TryGetValue(name, out newpath))
        {
            if (string.IsNullOrWhiteSpace(newpath))
                return null; //Don't push
            path = newpath;
        }
        return new Remote(Name, path);
    }

    string GenerateRemoteFlatGit(string name)
    {
        //Make sure remote for bare repos doesn't get double .git extension
        if (name.EndsWith(".git"))
            name = name.Substring(0, name.Length - ".git".Length);

        if (Path.StartsWith("ssh://", StringComparison.OrdinalIgnoreCase))
            return Path.TrimEnd('/') + "/" + name + ".git";
        else
            return System.IO.Path.Combine(Path, name + ".git");
    }

}
