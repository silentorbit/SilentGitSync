using SilentOrbit.GitSync;
using System;
using System.IO;

namespace SilentOrbit
{
    public class Program
    {
        const string defaultConfigFilename = "GitSync.json";

        static void Main(string[] args)
        {
            string configPath = null;
            if (args.Length == 0)
                configPath = defaultConfigFilename;
            else if(args.Length == 1)
                configPath = args[0];
            
            if(configPath == null)
            {
                Console.Error.WriteLine("Usage: GitSync.exe <path to GitConfig.json>");
                return;
            }

            var scanner = Scanner.LoadConfig(configPath);
            scanner.RunAllRemotes();
        }
    }
}
