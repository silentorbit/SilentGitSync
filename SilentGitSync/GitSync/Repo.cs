using static System.IO.Path;

namespace SilentOrbit.GitSync;

class Repo
{
    /// <summary>
    /// Local file or remote url
    /// </summary>
    public string Path { get; }

    public bool IsSSH { get; }

    public string SshLocalPath { get; }

    public string SshHost { get; }

    public bool HasUncommittedChanges()
    {
        if (IsBare)
            return false;

        var exit = RunGitOutput("status --porcelain", out var output);
        if (exit != 0)
        {
            Console.WriteLine(output);
            Console.WriteLine("git exit: " + exit);
            return true;
        }
        if (output == "")
            return false;

        Console.WriteLine(output);
        var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        return lines.Length > 0;
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
            return bare;
        }
    }

    public bool IsWorkTree { get; }

    public Repo(string path, string toStringPath = null)
    {
        this.Path = path ?? throw new ArgumentNullException();
        this.ToStringPath = toStringPath ?? path;

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

        IsWorkTree = DetermineWorkTree(Path, IsSSH);
    }

    static bool DetermineWorkTree(string path, bool isSSH)
    {
        if (isSSH)
            return false;

        var gitPath = Combine(path, ".git");
        if (Directory.Exists(gitPath))
            return false;
        if (File.Exists(gitPath) == false)
            return false; //Bare repo

        var content = File.ReadAllText(gitPath);
        if (content.StartsWith("gitdir: ") == false)
            throw new NotImplementedException();
        if (content.Contains(".git/modules/"))
            return false;
        if (content.Contains(".git/worktrees/"))
            return true;
        throw new NotImplementedException();
    }

    public string ToStringPath { get; }
    public override string ToString() => ToStringPath;

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

    /// <summary>
    /// Return non zero exist code on error
    /// </summary>
    internal int ConfigAllowError(string key, string value)
    {
        return RunGit("config --local " + key + " " + value);
    }

    internal void AddRemote(Remote remote)
    {
        RunGitThrow("remote add " + remote.Name + " \"" + remote.Git.Path + "\"");
    }

    public void Fetch(Remote remote)
    {
        RunGitThrow("fetch " + remote.Name);
    }

    internal void Push(Remote remote)
    {
        var branches = BranchList();
        // push --all "nas"
        // push -u --recurse-submodules=on-demand --progress "nas" refs/heads/master:refs/heads/master
        foreach (var branch in branches)
        {
            RunGit("rev-parse " + branch + " --", out string localHash);
            RunGit("rev-parse " + remote.Name + "/" + branch + " --", out string remoteHash);
            if (localHash == remoteHash && !string.IsNullOrWhiteSpace(localHash))
                continue;

            RunGitThrow("push -f -u " + remote.Name + " " + branch);
        }
    }

    public List<string> BranchList(bool remote = false)
    {
        var exit = RunGit("branch --list" + (remote ? " --all" : ""), out string output);
        if (exit != 0)
            throw new Exception("Failed to run: git branch --list");
        var lines = output.Split('\n');
        var branches = new List<string>();
        foreach (var l in lines)
        {
            var br = l.Trim(' ', '*', '\r', '+');
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

        using var p = new Process();
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

    int RunGitOutput(string args, out string output)
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
        psi.RedirectStandardOutput = true;

        using (var p = new Process())
        {
            p.StartInfo = psi;
            p.Start();
            p.WaitForExit();
            output = p.StandardOutput.ReadToEnd();
            return p.ExitCode;
        }
    }

    #endregion
}
