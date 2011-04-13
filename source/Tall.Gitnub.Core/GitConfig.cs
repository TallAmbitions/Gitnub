// Copyright (c) 2011 Tall Ambitions, LLC
// See included LICENSE for details.
namespace Tall.Gitnub.Core
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Class that can read from Git config files.
    /// </summary>
    public class GitConfig
    {
        private readonly GitConfigStore localConfig = LoadLocalConfig();
        private readonly GitConfigStore globalConfig = LoadGlobalConfig();

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
            return globalConfig.GetValues(name) ?? Enumerable.Empty<string>();
        }

        /// <summary>
        /// Gets all values from the local configuration of the given name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public IEnumerable<string> GetLocalValues(string name)
        {
            return localConfig.GetValues(name) ?? Enumerable.Empty<string>();
        }

        private static GitConfigStore LoadGlobalConfig()
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

        private static GitConfigStore LoadLocalConfig()
        {
            return LoadConfig(GetParentDirectories(), @".git\config");
        }

        private static IEnumerable<string> GetParentDirectories()
        {
            var dirInfo = new DirectoryInfo(Directory.GetCurrentDirectory());
            while(dirInfo != null)
            {
                yield return dirInfo.FullName;
                dirInfo = Directory.GetParent(dirInfo.FullName);
            }
        }

        private static GitConfigStore LoadConfig(IEnumerable<string> paths, string filename)
        {
            var configFile = paths.Select(path => Path.Combine(path, filename))
                                  .FirstOrDefault(File.Exists);
            if (configFile != null)
            {
                return new GitConfigStore(configFile);
            }
            return new GitConfigStore();
        }
    }
}