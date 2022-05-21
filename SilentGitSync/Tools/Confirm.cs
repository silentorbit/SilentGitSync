namespace SilentOrbit.Tools;

public static class Confirm
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
                    throw new UserAbortException();
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
                    throw new UserAbortException();
                default:
                    goto ask;
            }
        }
    }

    public static ConsoleKey ReadKey(string options)
    {
        //Empty previous key presses before prompting
        while (Console.KeyAvailable)
            Console.ReadKey();

        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("    " + options);
        Console.ResetColor();
        var key = Console.ReadKey().Key;
        Console.WriteLine();
        return key;
    }

}
