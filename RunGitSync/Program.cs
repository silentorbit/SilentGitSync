using SilentOrbit;

const string defaultConfigFilename = "GitSync.json";

string? configPath = null;
if (args.Length == 0)
    configPath = defaultConfigFilename;
else if (args.Length == 1)
    configPath = args[0];

if (configPath == null)
{
    Console.Error.WriteLine("Usage: GitSync.exe <path to GitConfig.json>");
    return;
}

var scanner = ConfigLoader.LoadConfig(configPath);
scanner.RunAllRemotes();
