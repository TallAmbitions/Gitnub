// Copyright (c) 2011 Tall Ambitions, LLC
// See included LICENSE for details.
namespace Tall.Gitnub.Core
{
    /// <summary>
    /// Interface that describes operations on Github's download area.
    /// </summary>
    public interface IDownloads
    {
        /// <summary>
        ///   Adds a file to the downloads available for a repository.
        /// </summary>
        /// <param name = "filename">The filename.</param>
        /// <param name = "description">The description.</param>
        /// <returns></returns>
        bool AddFile(string filename, string description = null);
    }
}