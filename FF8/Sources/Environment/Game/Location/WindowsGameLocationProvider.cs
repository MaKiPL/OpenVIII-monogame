using System;
using System.IO;
using Microsoft.Win32;

namespace FF8
{
    public sealed class WindowsGameLocationProvider : IGameLocationProvider
    {
        private const String SteamRegistyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 39150";
        private const String SteamGamePathTag = @"InstallLocation";

        public GameLocation GetGameLocation()
        {
            if (_hardcoded.FindGameLocation(out var gameLocation))
                return gameLocation;

            using (RegistryKey localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
            using (RegistryKey registryKey = localMachine.OpenSubKey(SteamRegistyPath))
            {
                if (registryKey != null)
                {
                    String installLocation = (String)registryKey.GetValue(SteamGamePathTag);
                    String dataPath = Path.Combine(installLocation, "Data", "lang-en");
                    if (Directory.Exists(dataPath))
                        return new GameLocation(dataPath);
                }
            }

            throw new DirectoryNotFoundException($"Cannot find game directory." +
                                                 $"Add your own path to the {nameof(WindowsGameLocationProvider)} type or fix a registration in the registry:" +
                                                 $"{SteamRegistyPath}.{SteamGamePathTag}");
        }

        private readonly HardcodedGameLocationProvider _hardcoded = new HardcodedGameLocationProvider(new[]
        {
            @"C:\Program Files (x86)\Steam\steamapps\common\FINAL FANTASY VIII\Data\lang-en",
            @"D:\SteamLibrary\steamapps\common\FINAL FANTASY VIII\Data\lang-en",
            @"D:\Steam\steamapps\common\FINAL FANTASY VIII\Data\lang-en",
        });
    }
}