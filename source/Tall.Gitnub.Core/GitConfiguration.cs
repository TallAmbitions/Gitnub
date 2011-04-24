// Copyright (c) 2011 Tall Ambitions, LLC
// See included LICENSE for details.
namespace Tall.Gitnub.Core
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Class that can read from Git config files.
    /// </summary>
    public class GitConfiguration
    {
        private readonly IConfigurationStore localConfiguration;
        private readonly IConfigurationStore globalConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="GitConfiguration"/> class.
        /// </summary>
        /// <param name="localConfiguration">The local config; if <c>null</c> then the default is used.</param>
        /// <param name="globalConfiguration">The global config; if <c>null</c> then the default is used.</param>
        public GitConfiguration(IConfigurationStore localConfiguration = null, IConfigurationStore globalConfiguration = null)
        {
            this.globalConfiguration = localConfiguration ?? LoadGlobalConfig();
            this.localConfiguration = globalConfiguration ?? LoadLocalConfig();
        }

        /// <summary>
        /// Gets the currently checked out branch.
        /// </summary>
        /// <returns>the current branch or null if HEAD is detached</returns>
        public string GetBranch()
        {
            var headFile = Path.Combine(Utility.FindGitDirectory(), "HEAD");
            var line = File.ReadAllLines(headFile).First();
            var match = Regex.Match(line, "^ref: (.*/)*(?'branch'.*)$");
            return match.Success ? match.Groups["branch"].Value : null;
        }

        /// <summary>
        /// Gets a value from the configuration.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public string GetValue(string name)
        {
            return this.GetLocalValues(name).SingleOrDefault() ?? this.GetGlobalValues(name).SingleOrDefault();
        }

        /// <summary>
        /// Gets a value from the global configuration.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public string GetGlobalValue(string name)
        {
            return this.GetGlobalValues(name).FirstOrDefault();
        }

        /// <summary>
        /// Gets a value from the local configuration.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public string GetLocalValue(string name)
        {
            return this.GetLocalValues(name).FirstOrDefault();
        }

        /// <summary>
        /// Gets all values from the configuration of the given name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public IEnumerable<string> GetValues(string name)
        {
            return this.GetGlobalValues(name).Concat(this.GetLocalValues(name));
        }

        /// <summary>
        /// Gets all values from the global configuration of the given name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public IEnumerable<string> GetGlobalValues(string name)
        {
            return this.globalConfiguration.GetValues(name) ?? Enumerable.Empty<string>();
        }

        /// <summary>
        /// Gets all values from the local configuration of the given name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public IEnumerable<string> GetLocalValues(string name)
        {
            return this.localConfiguration.GetValues(name) ?? Enumerable.Empty<string>();
        }

        private static IConfigurationStore LoadGlobalConfig()
        {
            var environmentVariables = new[] {"HOME", "HOMEPATH"};
            var specialFolders = new[]
                                     {
                                         Environment.SpecialFolder.ApplicationData,
                                         Environment.SpecialFolder.LocalApplicationData,
                                         Environment.SpecialFolder.Personal,
                                     };
            var paths = environmentVariables.Select(Environment.GetEnvironmentVariable)
                      .Concat(specialFolders.Select(Environment.GetFolderPath))
                      .Where(s => !String.IsNullOrEmpty(s));
            return LoadConfig(paths, @".gitconfig");
        }

        private static IConfigurationStore LoadLocalConfig()
        {
            return LoadConfig(new []{Utility.FindGitDirectory()}, @"config");
        }

        private static IConfigurationStore LoadConfig(IEnumerable<string> paths, string filename)
        {
            var configFile = paths.Select(path => Path.Combine(path, filename))
                                  .FirstOrDefault(File.Exists);
            if (configFile != null)
            {
                return new GitConfigurationStore(configFile);
            }
            return new GitConfigurationStore();
        }
    }
}