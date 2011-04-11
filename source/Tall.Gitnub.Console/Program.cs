// Copyright (c) 2011 Tall Ambitions, LLC
// See included LICENSE for details.
namespace Tall.Gitnub.Console
{
    using System;
    using Tall.Gitnub.Core;

    internal class Program
    {
        private static void Main(string[] args)
        {
            var upload = new Downloads(args[1], args[2], args[3]);
            Console.WriteLine(upload.AddFile(args[0]));
        }
    }
}