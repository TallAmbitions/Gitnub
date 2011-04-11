// Copyright (c) 2011 Tall Ambitions, LLC
// See included LICENSE for details.
namespace Tall.Gitnub.Core
{
    using System.Collections.Generic;

    /// <summary>
    /// Class that maps file extensions to mime types.
    /// </summary>
    public static class MimeMap
    {
        private static readonly IDictionary<string, string> map =
            new Dictionary<string, string>
                {
                    {@"gz", @"application/x-gzip"},
                    {@"js", @"application/javascript"},
                    {@"pdf", @"application/pdf"},
                    {@"zip", @"application/zip"},
                };

        /// <summary>
        /// Gets the MIME type for extension.
        /// </summary>
        /// <param name="extension">The extension.</param>
        /// <returns></returns>
        public static string GetMimeTypeForExtension(string extension)
        {
            string mimeType;
            if (!map.TryGetValue(extension.TrimStart('.'), out mimeType))
            {
                mimeType = "application/octet-stream";
            }
            return mimeType;
        }
    }
}