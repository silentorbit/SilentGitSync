using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilentOrbit.Tools
{
    static class Confirm
    {
        public static bool Action(string message)
        {
        ask:
            Console.BackgroundColor = ConsoleColor.Cyan;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.WriteLine(message);
            Console.ResetColor();
            while (true)
            {
                var key = ReadKey("[Y]es, [R]etry or [A]bort");
                switch (key)
                {
                    case ConsoleKey.Y:
                        return true;
                    case ConsoleKey.R:
                        return false;
                    case ConsoleKey.A:
                        throw new Exception("User aborted");
                    default:
                        goto ask;
                }
            }
        }

        public static bool Retry(string message)
        {
        ask:
            Console.BackgroundColor = ConsoleColor.Cyan;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.WriteLine(message);
            Console.ResetColor();
            while (true)
            {
                var key = ReadKey("[R]etry, [I]gnore or [A]bort");
                switch (key)
                {
                    case ConsoleKey.R:
                        return true;
                    case ConsoleKey.I:
                        return false;
                    case ConsoleKey.A:
                        throw new Exception("User aborted");
                    default:
                        goto ask;
                }
            }
        }

        public static ConsoleKey ReadKey(string options)
        {
            Console.ResetColor();
            Console.Write("    " + options + " ...");
            var key = Console.ReadKey().Key;
            Console.WriteLine();
            return key;
        }

    }
}
