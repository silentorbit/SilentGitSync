using System.IO;

namespace SilentOrbit.GitSync.Scanning;

static class RepoSync
{
    /// <summary>
    /// Update remote config and push changes
    /// </summary>
    public static void SyncRepo(Repo repo, Remote target, SyncConfig config)
    {
        Console.WriteLine(" === " + repo.ToStringPath);

        while (repo.HasUncommittedChanges())
        {
            //source.Status();

            var psi = new ProcessStartInfo(@"C:\Program Files (x86)\GitExtensions\GitExtensions.exe", "commit");
            psi.WorkingDirectory = repo.Path;
            using (var p = Process.Start(psi))
            {
                p.WaitForExit();
            }

            if (repo.HasUncommittedChanges())
            {
                //Still uncommited changes
                if (Confirm.Retry("Detected uncommitted changes in " + repo.Path))
                    continue;
            }

            break;
        }

        //Remote config
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("     [" + target.Name + "] ");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(target.Git);
        Console.ResetColor();

        repo.ConfigGlobalSafe();
        repo.Config("pack.packSizeLimit", "1g");

    retry:
        try
        {
            var existed = target.Git.IsSSH || target.Git.Exists();

            FixRemote(repo, target);

            //Fetch and Merge
            if (config.FetchBeforePush && existed && repo.HasUncommittedChanges() == false)
            {
                repo.Fetch(target);
                /* Disabled merge since we can't be sure what branch we're on
                 * Better to keep this manually
                if (source.IsBare == false)
                {
                    while (true)
                    {
                        try
                        {
                            source.Merge(target);
                            break;
                        }
                        catch (Exception ex)
                        {
                            Console.Error.WriteLine(ex.Message);
                            var key = Confirm.ReadKey("[S]kip or [R]etry");
                            if (key == ConsoleKey.S)
                                break;
                        }
                    }
                }*/

                while (repo.HasUncommittedChanges())
                {
                    var psi = new ProcessStartInfo(@"C:\Program Files (x86)\GitExtensions\GitExtensions.exe", "commit");
                    psi.WorkingDirectory = repo.Path;
                    using (var p = Process.Start(psi))
                    {
                        p.WaitForExit();
                    }

                    if (Confirm.Retry("Detected uncommitted changes in " + repo.Path))
                        continue;

                    break;
                }
            }

            //Push
            repo.Push(target);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            var key = Confirm.ReadKey("[S]kip or [R]etry");
            if (key != ConsoleKey.S)
                goto retry;
        }
    }

    /// <summary>
    /// Create and add remote location if missing.
    /// Adds/updates remote on source repo.
    /// </summary>
    static void FixRemote(Repo source, Remote target)
    {
        var targetGit = target.Git;

        if (targetGit.CanModify)
        {
        retryTarget:
            //Target
            if (targetGit.Exists() == false)
            {
                if (Confirm.Action("Init remote repo at " + targetGit))
                    targetGit.InitBare();
                else
                    goto retryTarget;
            }

            //New config needed to allow changing config on remote "untrusted FS owner"
            //Neither of these workarounds should be applied automatically
            //Workaround 1: add path to safe.directory
            //if (targetGit.IsSSH == false)
            //    targetGit.RunGitThrow("config --global --add safe.directory \"" + targetGit.Path.Replace('\\', '/') + "\"");
            //Workaround 2: Add once
            // git config --global --add safe.directory *

            //Pack Size Max 1GB
            //Will come in effect on the next "git gc"
            targetGit.ConfigAllowError("pack.packSizeLimit", "1g");
            targetGit.ConfigAllowError("receive.denyNonFastForwards", "true");
            targetGit.ConfigAllowError("receive.denyDeletes", "true");
        }

    //Add remote and push
    retryRemote:
        var remoteUrl = source.RemoteGetUrl(target.Name);
        if (remoteUrl != targetGit.Path)
        {
            if (remoteUrl == null)
            {
                if (Confirm.Action("Adding remote " + target))
                    source.AddRemote(target);
                else
                    goto retryRemote;
            }
            else
            {
                if (Confirm.Action("Modifying remote " + target.Name + "\n from " + remoteUrl + "\n to   " + target.Git.Path))
                    source.RemoteSetUrl(target);
                else
                    goto retryRemote;
            }

            remoteUrl = source.RemoteGetUrl(target);
            if (remoteUrl != targetGit.Path)
                throw new Exception("Failed to update the remote: " + target + " = " + targetGit.Path);
        }
    }
}
