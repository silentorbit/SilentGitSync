using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace SilentOrbit.GitSync
{
    class Repo
    {
        public string Path { get; }

        public bool IsSSH { get; }

        public string SshLocalPath { get; }

        public string SshHost { get; }

        public bool HasUncommittedChanges()
        {
            if (IsBare)
                return false;
            return RunGit("diff-index --quiet HEAD --") == 1;
        }

        public void Status()
        {
            RunGit("status --short --branch");
        }

        /// <summary>
        /// If we can create and modify remote repos
        /// </summary>
        public bool CanModify { get; }
        public bool IsBare
        {
            get
            {
                var bare = Path.EndsWith(".git");
#if DEBUG
                switch (Path)
                {
                    case "R:\\Git\\Adductor\\DriveSync.git":
                        Debug.Assert(bare == true);
                        break;
                    default:
                        Debug.Assert(bare == false);
                        break;
                }
#endif
                return bare;
            }
        }

        public Repo(string path)
        {
            this.Path = path;

            {
                var ma = new Regex("^ssh://[a-z]+@([a-z.]+)/(.*)$").Match(path);
                if (ma.Success)
                {
                    IsSSH = true;
                    SshHost = ma.Groups[1].Value;
                    SshLocalPath = ma.Groups[2].Value;
                }
            }
            {
                var ma = new Regex("^[a-z]+@([a-z.]+):(.*)$").Match(path);
                if (ma.Success)
                {
                    IsSSH = true;
                    SshHost = ma.Groups[1].Value;
                    SshLocalPath = ma.Groups[2].Value;
                }
            }

            Debug.Assert((path.Contains("@") || path.Contains("ssh://")) == IsSSH);
            CanModify = IsSSH == false;
        }

        public override string ToString() => Path;

        public bool Exists()
        {
            if (IsSSH)
            {
                int result = RunSsh("'[ -d " + EscapeSpace(SshLocalPath) + " ]'");
                return result == 0;
            }
            return Directory.Exists(Path);
        }

        internal void InitBare()
        {
            if (IsSSH)
                RunGitThrow("init --bare \"" + SshLocalPath + "\"");
            else
                RunGitThrow("init --bare \"" + Path + "\"");
        }

        internal string RemoteGetUrl(Remote remote) => RemoteGetUrl(remote.Name);

        /// <summary>
        /// Return the address to the remote.
        /// </summary>
        /// <param name="remote"></param>
        /// <returns></returns>
        internal string RemoteGetUrl(string remote)
        {
            int res = RunGit("remote get-url " + remote, out string output);
            if (res != 0)
                return null;
            output = output.Trim(' ', '\r', '\n');
            return output;
        }

        internal void RemoteSetUrl(Remote remote)
        {
            RunGitThrow("remote set-url " + remote.Name + " \"" + remote.Git.Path + "\"");
        }

        internal void Config(string key, string value)
        {
            RunGitThrow("config --local " + key + " " + value);
        }

        internal void AddRemote(Remote remote)
        {
            RunGitThrow("remote add " + remote.Name + " \"" + remote.Git.Path + "\"");
        }

        public void Fetch(Remote remote)
        {
            RunGitThrow("fetch " + remote.Name);
        }

        public void Merge(Remote remote)
        {
            RunGitThrow("merge --no-commit " + remote.Name + "/master");
        }

        internal void Push(Remote remote)
        {
            var branches = BranchList();
            // push --all "nas"
            // push -u --recurse-submodules=on-demand --progress "nas" refs/heads/master:refs/heads/master
            foreach (var branch in branches)
                RunGitThrow("push -u " + remote.Name + " " + branch);
        }

        public List<string> BranchList()
        {
            var exit = RunGit("branch --list", out string output);
            if (exit != 0)
                throw new Exception("Failed to run: git branch --list");
            var lines = output.Split('\n');
            var branches = new List<string>();
            foreach (var l in lines)
            {
                var br = l.Trim(' ', '*', '\r');
                if (br == "")
                    continue;
                branches.Add(br);
            }
            return branches;
        }

        #region Git and SSH commands

        static string Escape(string arguments)
        {
            return arguments.Replace(@"\", @"\\").Replace("\"", "\\\"");
        }

        static string EscapeSpace(string arguments)
        {
            return arguments.Replace(" ", @"\ ");
        }

        int RunSsh(string args)
        {
            if (IsSSH == false)
                throw new InvalidOperationException();

            using (var p = new Process())
            {
                p.StartInfo = new ProcessStartInfo
                {
                    FileName = "ssh",
                    Arguments = SshHost + " " + args,
                    UseShellExecute = false,
                };
                p.Start();
                p.WaitForExit();
                return p.ExitCode;
            }
        }

        int RunGit(string args, out string output)
        {
            var psi = new ProcessStartInfo();

            if (IsSSH)
            {
                psi.FileName = "ssh";
                string cdPath = "cd \"" + SshLocalPath + "\";";
                psi.Arguments = SshHost + " \"" + Escape(cdPath) + "git " + Escape(args) + "\"";
            }
            else
            {
                psi.WorkingDirectory = Path;
                psi.FileName = "git";
                psi.Arguments = args;
            }
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            using (var p = new Process())
            {
                p.StartInfo = psi;
                p.Start();
                //Warning: Make sure not reading output doesn't prevent the process from exiting
                output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
                return p.ExitCode;
            }
        }

        void RunGitThrow(string args)
        {
            int result = RunGit(args);
            if (result != 0)
                throw new Exception("Command failed: git " + args);
        }

        int RunGit(string args)
        {
            var psi = new ProcessStartInfo();

            bool gitWorkDir = args.StartsWith("init ") == false;

            if (IsSSH)
            {
                psi.FileName = "ssh";
                string cdPath = gitWorkDir ? "cd \"" + SshLocalPath + "\";" : "";
                psi.Arguments = SshHost + " \"" + Escape(cdPath) + "git " + Escape(args) + "\"";
            }
            else
            {
                if (gitWorkDir)
                    psi.WorkingDirectory = Path;
                psi.FileName = "git";
                psi.Arguments = args;
            }
            psi.UseShellExecute = false;
            using (var p = new Process())
            {
                p.StartInfo = psi;
                p.Start();
                p.WaitForExit();
                return p.ExitCode;
            }
        }

        #endregion
    }
}
