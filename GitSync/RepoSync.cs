using SilentOrbit.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SilentOrbit.GitSync
{
    static class RepoSync
    {
        public static void SyncRepo(Repo source, Remote target)
        {
            Console.WriteLine(" === " + source);

            while (source.HasUncommittedChanges())
            {
                source.Status();

                if (Confirm.Retry("Detected uncommited changes in " + source.Path))
                    continue;
                break;
            }

            Console.WriteLine(" === [" + target.Name + "] " + target.Git);

            source.Config("pack.packSizeLimit", "1g");

        retry:
            try
            {
                FixRemote(source, target);
                source.Push(target);
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

                //Pack Size Max 1GB
                //Will come in effect on the next "git gc"
                targetGit.Config("pack.packSizeLimit", "1g");
                targetGit.Config("receive.denyNonFastForwards", "true");
                targetGit.Config("receive.denyDeletes", "true");
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
}
