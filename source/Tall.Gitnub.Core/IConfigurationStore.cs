// Copyright (c) 2011 Tall Ambitions, LLC
// See included LICENSE for details.
namespace Tall.Gitnub.Core
{
    using System.Collections.Generic;

    /// <summary>
    /// Interface describing a repository of configuration information.
    /// </summary>
    public interface IConfigurationStore
    {
        /// <summary>
        ///   Gets the values for the given name.
        /// </summary>
        /// <param name = "name">The name.</param>
        /// <returns></returns>
        IEnumerable<string> GetValues(string name);
    }
}