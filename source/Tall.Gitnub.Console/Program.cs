// Copyright (c) 2011 Tall Ambitions, LLC
// See included LICENSE for details.
namespace Tall.Gitnub.Console
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using Tall.Gitnub.Core;
    using System.Linq;

    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Usage();
                return;
            }

            //TODO: proper option parsing
            if (args[0].Equals("upload", StringComparison.OrdinalIgnoreCase))
            {
                Upload(args.Skip(1).ToArray());
            }
            else
            {
                Console.WriteLine("Unknown action '{0}'.", args[0]);
                Usage();
            }
        }

        private static void Upload(string[] args)
        {
            if (args.Length < 2)
            {
                Usage();
                return;
            }

            var myArgs = new string[4];
            Array.Copy(args.ToArray(), myArgs, Math.Min(args.Length, myArgs.Length));

            var config = new GitConfiguration();
            var upload = new Downloads(myArgs[1], myArgs[2] ?? config.GetValue("github.user"), myArgs[3] ?? config.GetValue("github.token"));
            Console.WriteLine(upload.AddFile(myArgs[0]));
        }

        private static void Usage() {
            Console.WriteLine("Usage:\t {0} upload <filename> <repository> [username] [token]",
                              Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location));
            Console.WriteLine("\nOptional parameters are fetched from Git's configuration using 'github.user' and 'github.token' respectively.");
        }

    }
}