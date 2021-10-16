using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilentOrbit.GitSync
{
    public class Scanner
    {
        readonly Config config;

        public static Scanner LoadConfig(string jsonConfigPath)
        {
            var json = File.ReadAllText(jsonConfigPath, Encoding.UTF8);
            var config = JsonConvert.DeserializeObject<Config>(json);

            return new Scanner(config);
        }

        public Scanner(Config config)
        {
            this.config = config;

            config.Validate();
        }

        public void RunAllRemotes()
        {
            foreach (var remoteName in config.Remote.Keys)
                RunRemote(remoteName);
        }

        public void RunRemote(string remoteName)
        {
            foreach (var repo in config.Repo)
            {
                RunRepo(repo, remoteName);
            }
        }

        void RunRepo(Config.RepoConfig config, string remoteName)
        {
            if (config.Remote.Contains(remoteName) == false)
                return;

            var remoteBase = new RemoteBase(this.config, remoteName);

            if (config.RecursiveOnly)
            {
                Scan(config.Path, remoteBase, skipRoot: true);
            }
            else if (config.Recursive)
            {
                Scan(config.Path, remoteBase);
            }
            else
            {
                //Single repo
                var gitRepo = new Repo(config.Path);
                if (gitRepo.IsWorkTree)
                    return; //Skip worktrees as their main repo will be taken care of

                var remote = remoteBase.GenerateRemote(gitRepo);
                RepoSync.SyncRepo(gitRepo, remote);
            }
        }

        static void Scan(string sourceBase, RemoteBase remoteBase, bool skipRoot = false)
        {
            foreach (var gitPath in ScanGit(sourceBase))
            {
                if (skipRoot)
                {
                    if (Path.GetDirectoryName(gitPath) == sourceBase)
                        continue;
                }
                if (gitPath.StartsWith(@"C:\Users\peter\Adductor\DriveSync\My Drive"))
                    continue;

                var repoPath = Path.GetDirectoryName(gitPath);
                var source = new Repo(repoPath);
                if (source.IsWorkTree)
                    continue; //Skip worktrees as their main repo will be taken care of

                var target = remoteBase.GenerateRemote(source);
                RepoSync.SyncRepo(source, target);
            }
        }

        static IEnumerable<string> ScanGit(string root)
        {
            var list = new List<string>();

            foreach (var path in Directory.EnumerateDirectories(root, ".git", SearchOption.AllDirectories))
                list.Add(path);

            foreach (var path in Directory.EnumerateFiles(root, ".git", SearchOption.AllDirectories))
                list.Add(path);

            //Reverese sort order to make sure sub repos are synchronized before the main repos are
            list.Sort();
            list.Reverse();
            return list;
        }
    }
}
