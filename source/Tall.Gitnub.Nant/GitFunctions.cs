// Copyright (c) 2011 Tall Ambitions, LLC
// See included LICENSE for details.
namespace Tall.Gitnub.Nant
{
    using System;
    using System.Collections.Generic;
    using NAnt.Core;
    using NAnt.Core.Attributes;
    using Tall.Gitnub.Core;
    using System.Linq;

    /// <summary>
    ///   Nant functions for dealing with git.
    /// </summary>
    [FunctionSet("git", "Git")]
    public class GitFunctions : FunctionSetBase
    {
        private readonly GitConfiguration configuration = new GitConfiguration();

        public GitFunctions(Project project, PropertyDictionary properties)
            : base(project, properties) {}

        /// <summary>
        ///   Reads a value from the git configuration.
        /// </summary>
        /// <param name = "setting">The setting to read.</param>
        /// <returns></returns>
        [Function("readconfig")]
        public string ReadConfig(string setting)
        {
            return this.configuration.GetValue(setting);
        }

        /// <summary>
        ///   Reads a value from the local git configuration.
        /// </summary>
        /// <param name = "setting">The setting to read.</param>
        /// <returns></returns>
        [Function("readconfig-local")]
        public string ReadLocalConfig(string setting)
        {
            return this.configuration.GetLocalValue(setting);
        }

        /// <summary>
        ///   Reads a value from the global git configuration.
        /// </summary>
        /// <param name = "setting">The setting to read.</param>
        /// <returns></returns>
        [Function("readconfig-global")]
        public string ReadGlobalConfig(string setting)
        {
            return this.configuration.GetGlobalValue(setting);
        }

        /// <summary>
        ///   Reads a value from the git configuration.
        /// </summary>
        /// <param name = "setting">The setting to read.</param>
        /// <returns></returns>
        [Function("readconfig-all")]
        public string ReadAllConfig(string setting)
        {
            return Join(this.configuration.GetValues(setting));
        }

        /// <summary>
        ///   Reads a value from the local git configuration.
        /// </summary>
        /// <param name = "setting">The setting to read.</param>
        /// <returns></returns>
        [Function("readconfig-all-local")]
        public string ReadAllLocalConfig(string setting)
        {
            return Join(this.configuration.GetLocalValues(setting));
        }

        /// <summary>
        ///   Reads a value from the global git configuration.
        /// </summary>
        /// <param name = "setting">The setting to read.</param>
        /// <returns></returns>
        [Function("readconfig-all-global")]
        public string ReadAllGlobalConfig(string setting)
        {
            return Join(this.configuration.GetGlobalValues(setting));
        }

        private string Join(IEnumerable<string> values)
        {
            return String.Join(",", values.ToArray());
        }
    }
}