using System.Text;
using System.Text.Json;
using SilentOrbit.GitSync;

namespace SilentOrbit;

class ConfigLoader
{
    public static Scanner LoadConfig(string jsonConfigPath)
    {
        var json = File.ReadAllText(jsonConfigPath, Encoding.UTF8);
        var config = JsonSerializer.Deserialize<SyncConfig>(json);

        return new Scanner(config);
    }
}
