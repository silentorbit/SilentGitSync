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

        public void Run()
        {
            //Run in remote order
            foreach (var kv in config.Remote)
            {
                RunRemote(remoteName: kv.Key, remotePath: kv.Value);
            }
        }

        void RunRemote(string remoteName, string remotePath)
        {
            //Skip disconnected drives
            if (ConnectedDrive(remotePath) == false)
            {
                Console.WriteLine("Skipping not connected drive: " + remoteName + ": " + remotePath);
                return;
            }

            foreach (var repo in config.Repo)
            {
                if (ConnectedDrive(repo.Path) == false)
                {
                    Console.WriteLine("Skipping not connected drive: " + remoteName + ": " + repo.Path);
                    continue;
                }

                RunRepo(repo, remoteName);
            }
        }

        /// <summary>
        /// Return true if a local drive is connected.
        /// Return true on all remote repos.
        /// </summary>
        static bool ConnectedDrive(string remotePath)
        {
            var index = remotePath.IndexOf(@":\");
            if (index == -1)
                return true; //Not a local drive

            var drive = remotePath.Substring(0, index + 2);
            return Directory.Exists(drive);
        }

        void RunRepo(Config.RepoConfig repo, string remoteName)
        {
            if (repo.Remote.Contains(remoteName) == false)
                return;

            var remoteBase = new Remote(config, remoteName);

            if (repo.RecursiveOnly)
            {
                Scan(repo.Path, remoteBase, skipRoot: true);
            }
            else if (repo.Recursive)
            {
                Scan(repo.Path, remoteBase);
            }
            else
            {
                //Single repo
                var gitRepo = new Repo(repo.Path);
                if (gitRepo.IsWorkTree)
                    return; //Skip worktrees as their main repo will be taken care of

                var remote = remoteBase.GenerateRemote(gitRepo);
                RepoSync.SyncRepo(gitRepo, remote);
            }
        }

        static void Scan(string sourceBase, Remote remoteBase, bool skipRoot = false)
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
