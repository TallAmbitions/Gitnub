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
            return this.GetLocalValue(name) ?? this.GetGlobalValue(name);
        }

        /// <summary>
        /// Gets a value from the global configuration.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public string GetGlobalValue(string name)
        {
            return globalConfig[name];
        }

        /// <summary>
        /// Gets a value from the local configuration.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public string GetLocalValue(string name)
        {
            return localConfig[name];
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
                //TODO: Handle subsections format: [section "subsection"] (including case-sensitivity)
                var sectionRegex = new Regex(@"(\[(?<section>[\w\.\-]+)\])|(?<key>[\w\-]+)\s*=(?<value>.*)");
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
                        }
                        else
                        {
                            //TODO: Escaped characters, quotes, line-continuations
                            //TODO: Multi-values
                            var key = String.Format("{0}.{1}", section, match.Groups["key"].Value.Trim());
                            result[key] = match.Groups["value"].Value.Trim();
                        }
                    }
                }
            }
            return result;
        }
    }
}