// Copyright (c) 2011 Tall Ambitions, LLC
// See included LICENSE for details.
namespace Tall.Gitnub.Core
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Class that can read from Git config files.
    /// </summary>
    public class GitConfig
    {
        private readonly NameValueCollection localConfig = LoadLocalConfig();
        private readonly NameValueCollection globalConfig = LoadGlobalConfig();

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

        private static NameValueCollection LoadGlobalConfig()
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

        private static NameValueCollection LoadLocalConfig()
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

        private static NameValueCollection LoadConfig(IEnumerable<string> paths, string filename)
        {
            var result = new NameValueCollection(StringComparer.OrdinalIgnoreCase);
            var configFile = paths.Select(path => Path.Combine(path, filename))
                                  .FirstOrDefault(File.Exists);
            if(configFile != null)
            {
                var section = string.Empty;
                var sectionRegex = 
                    new Regex(@"(\[\s*(?<section>[\w\.\-]+)(\s+""(?<subsection>[\w\.\-]+)"")?\s*\])" +
                              @"|(?<key>[\w\-]+)\s*=(?<value>.*)");
                var lines = File.ReadAllLines(configFile)
                                .Select(line => line.Split('#', ';').First().Trim());

                foreach (var line in lines)
                {
                    var match = sectionRegex.Match(line);
                    if (match != Match.Empty)
                    {
                        var sectionGroup = match.Groups["section"];
                        if (sectionGroup.Success)
                        {
                            section = sectionGroup.Value.Trim();
                            var subsection = match.Groups["subsection"];
                            if (subsection.Success)
                            {
                                //TODO: Case-sensitive!
                                section = String.Join(".", new[] {section, subsection.Value});
                            }
                        }
                        else
                        {
                            //TODO: Escaped characters, quotes, line-continuations
                            var key = String.Format("{0}.{1}", section, match.Groups["key"].Value.Trim());
                            var value = match.Groups["value"].Value.Trim();
                            if (String.IsNullOrEmpty(value))
                            {
                                value = "true";
                            }
                            result.Add(key, value);
                        }
                    }
                }
            }
            return result;
        }
    }
}