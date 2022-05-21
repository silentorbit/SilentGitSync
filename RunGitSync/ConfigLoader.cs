using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
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
