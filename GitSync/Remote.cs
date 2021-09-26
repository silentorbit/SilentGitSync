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
        public readonly Config SyncConfig;

        public string Name { get; set; }
        public Repo Git { get; set; }

        public Remote(Config config, string remoteName, string remotePath = null)
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

            if (Git.Path.StartsWith("ssh://", StringComparison.OrdinalIgnoreCase))
                return Git.Path.TrimEnd('/') + "/" + name + ".git";
            else
                return Path.Combine(Git.Path, name + ".git");
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
}
