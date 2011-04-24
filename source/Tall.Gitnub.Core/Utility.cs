// Copyright (c) 2011 Tall Ambitions, LLC
// See included LICENSE for details.
namespace Tall.Gitnub.Core
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    internal static class Utility
    {
        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> source)
        {
            return source ?? Enumerable.Empty<T>();
        }

        public static IEnumerable<T> Expand<T>(T initial, Func<T,T> expander)
        {
            return Expand(initial, expander, _ => true);
        }

        public static IEnumerable<T> Expand<T>(T initial, Func<T,T> expander, Func<T, bool> terminate)
        {
            var value = initial;
            while (!terminate(value))
            {
                yield return value;
                value = expander(value);
            }
        }

        public static string FindGitDirectory()
        {
            return Expand(new DirectoryInfo(Directory.GetCurrentDirectory()), d => d.Parent, d => d == null)
                .Select(d => Path.Combine(d.FullName, @".git"))
                .FirstOrDefault(Directory.Exists);
        }
    }
}