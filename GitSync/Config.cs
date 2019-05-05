using System;
using System.Collections.Generic;
using System.Text;

namespace SilentOrbit.GitSync
{
    public class Config
    {
        /// <summary>
        /// Key: git remote name
        /// Value: remote path/url
        /// </summary>
        public Dictionary<string, string> Remote { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Scan these paths and subdirectories for any git repo.
        /// </summary>
        public List<RepoConfig> Repo { get; set; } = new List<RepoConfig>();

        /// <summary>
        /// Map generated remote url to custom url.
        /// </summary>
        public Dictionary<string, string> RemoteAlias { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Check configuration for mistakes
        /// </summary>
        public void Validate()
        {
            //Make sure all repo remotes exists in Remote.
            foreach (var r in Repo)
            {
                foreach (var remote in r.Remote)
                {
                    if (Remote.ContainsKey(remote) == false)
                        throw new ArgumentException("Remote " + remote + " references in " + r.Path + " does not exist in list of remotes");
                }
            }
        }

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

            public List<string> Remote { get; set; } = new List<string>();

            public override string ToString()
            {
                return Path + " [" + (Recursive ? "recursive" : "") + (RecursiveOnly ? "recursiveOnly" : "") + "] " + string.Join(", ", Remote);
            }
        }
    }
}
