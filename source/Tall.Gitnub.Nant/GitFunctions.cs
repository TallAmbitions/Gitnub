// Copyright (c) 2011 Tall Ambitions, LLC
// See included LICENSE for details.
namespace Tall.Gitnub.Nant
{
    using System.Diagnostics;
    using System.IO;
    using NAnt.Core;
    using NAnt.Core.Attributes;

    /// <summary>
    /// Nant functions for dealing with git.
    /// </summary>
    [FunctionSet("git", "Git")]
    public class GitFunctions : FunctionSetBase
    {
        public GitFunctions(Project project, PropertyDictionary properties)
            : base(project, properties) {}

        /// <summary>
        /// Reads a value from the git config.
        /// </summary>
        /// <param name="setting">The setting to read.</param>
        /// <returns></returns>
        [Function("readconfig")]
        public string ReadConfig(string setting)
        {
            return RunGit("config " + setting).TrimEnd('\r', '\n');
        }

        /// <summary>
        /// Reads a value from the local git config.
        /// </summary>
        /// <param name="setting">The setting to read.</param>
        /// <returns></returns>
        [Function("readconfig-local")]
        public string ReadLocalConfig(string setting)
        {
            return RunGit("config --local " + setting).TrimEnd('\r', '\n');
        }

        /// <summary>
        /// Reads a value from the global git config.
        /// </summary>
        /// <param name="setting">The setting to read.</param>
        /// <returns></returns>
        [Function("readconfig-global")]
        public string ReadGlobalConfig(string setting)
        {
            return RunGit("config --global " + setting).TrimEnd('\r', '\n');
        }

        private string RunGit(string arguments) {
            var gitPath = this.GetGitPath();
            var startInfo = new ProcessStartInfo(gitPath, arguments)
                                {
                                    RedirectStandardOutput = true,
                                    UseShellExecute = false,
                                };
            using (var process = Process.Start(startInfo))
            {
                return process.StandardOutput.ReadToEnd();
            }
        }

        private string GetGitPath()
        {
            var path = this.Project.Properties["tools.git.path"] ?? string.Empty;
            var exe = "git.exe";
            if(!string.IsNullOrEmpty(path))
            {
                exe = Project.GetFullPath(Path.Combine(path, exe));
            }
            return exe;
        }
    }
}