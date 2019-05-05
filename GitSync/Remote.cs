using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SilentOrbit.GitSync
{
    /// <summary>
    /// Remote name and base path
    /// </summary>
    class Remote
    {
        readonly Config config;

        public string Name { get; set; }
        public Repo Git { get; set; }

        public Remote(Config config, string remoteName, string remotePath = null)
        {
            this.config = config;
            this.Name = remoteName;
            if (remotePath == null)
                remotePath = config.Remote[Name];
            this.Git = new Repo(remotePath);
        }

        public override string ToString()
        {
            return Name + ": " + Git.Path;
        }

        public Remote GenerateRemote(Repo repo)
        {
            var path = GenerateRemoteFlatGit(repo.Path);
            if (config.RemoteAlias.TryGetValue(path, out var newPath))
                path = newPath;
            return new Remote(config, Name, path);
        }

        string GenerateRemoteFlatGit(string sourceRepo)
        {
            var name = Path.GetFileName(sourceRepo) + ".git";
            if (Git.Path.StartsWith("ssh://", StringComparison.OrdinalIgnoreCase))
                return Git.Path.TrimEnd('/') + "/" + name;
            else
                return Path.Combine(Git.Path, name);
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

            return new Remote(config, Name, path);
        }
    }
}
