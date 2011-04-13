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
    /// Reads a Git config file into memory.
    /// </summary>
    public class GitConfigStore
    {
        private readonly NameValueCollection standardConfig = new NameValueCollection(StringComparer.OrdinalIgnoreCase);
        private readonly NameValueCollection subsectionConfig = new NameValueCollection(StringComparer.Ordinal);

        /// <summary>
        /// Initializes a new instance of the <see cref="GitConfigStore"/> class.
        /// </summary>
        public GitConfigStore() {}

        /// <summary>
        /// Initializes a new instance of the <see cref="GitConfigStore"/> class.
        /// </summary>
        /// <param name="configFile">The config file.</param>
        public GitConfigStore(string configFile)
        {
            if (!String.IsNullOrEmpty(configFile))
            {
                this.LoadConfig(File.ReadAllLines(configFile));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GitConfigStore"/> class.
        /// </summary>
        /// <param name="lines">The lines of the configuration file.</param>
        public GitConfigStore(IEnumerable<string> lines)
        {
            this.LoadConfig(lines.EmptyIfNull());
        }

        /// <summary>
        ///   Gets the values for the given name.
        /// </summary>
        /// <param name = "name">The name.</param>
        /// <returns></returns>
        public IEnumerable<string> GetValues(string name)
        {
            return this.standardConfig.GetValues(name).EmptyIfNull()
                .Concat(this.subsectionConfig.GetValues(name).EmptyIfNull());
        }

        private void LoadConfig(IEnumerable<string> lines)
        {
            var section = string.Empty;
            var sectionRegex =
                new Regex(@"(\[\s*(?<section>[\w\.\-]+)(\s+""(?<subsection>[\w\.\-]+)"")?\s*\])" +
                          @"|(?<key>[\w\-]+)\s*=(?<value>.*)");
            // Remove comments
            lines = lines.Select(line => line.Split('#', ';').First().Trim());

            var caseSensitive = false;
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
                        caseSensitive = subsection.Success;
                        if (subsection.Success)
                        {
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
                        (caseSensitive ? this.subsectionConfig : this.standardConfig).Add(key, value);
                    }
                }
            }
        }
    }
}