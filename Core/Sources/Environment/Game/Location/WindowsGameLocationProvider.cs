using System;
using System.IO;
using Microsoft.Win32;

namespace OpenVIII
{
    public sealed class WindowsGameLocationProvider : IGameLocationProvider
    {
        private const String SteamRegistyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 39150";
        private const String SteamGamePathTag = @"InstallLocation";
        /// <summary>
        /// Supplied from LordUrQuan
        /// </summary>
        private const String CD2000RegistyPath = @"SOFTWARE\Wow6432Node\Square Soft, Inc\FINAL FANTASY VIII\1.00";
        private const String CD2000GamePathTag = @"AppPath";

        public GameLocation GetGameLocation()
        {
            if (_hardcoded.FindGameLocation(out var gameLocation))
                return gameLocation;

            foreach (RegistryView registryView in new RegistryView[] { RegistryView.Registry32, RegistryView.Registry64 })
            {
                using (RegistryKey localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, registryView))
                using (RegistryKey registryKey = localMachine.OpenSubKey(SteamRegistyPath))
                {
                    if (registryKey != null)
                    {
                        String installLocation = (String)registryKey.GetValue(SteamGamePathTag);
                        String dataPath = installLocation;//Path.Combine(installLocation, "Data", "lang-en");
                        if (Directory.Exists(dataPath))
                            return new GameLocation(dataPath);
                    }
                }
                using (RegistryKey localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, registryView))
                using (RegistryKey registryKey = localMachine.OpenSubKey(CD2000RegistyPath))
                {
                    if (registryKey != null)
                    {
                        String installLocation = (String)registryKey.GetValue(CD2000GamePathTag);
                        String dataPath = installLocation; //Path.Combine(installLocation, "Data"); //no lang-en on cd version.
                        if (Directory.Exists(dataPath))
                            return new GameLocation(dataPath);
                    }
                }

            }

                throw new DirectoryNotFoundException($"Cannot find game directory." +
                                                 $"Add your own path to the {nameof(WindowsGameLocationProvider)} type or fix a registration in the registry:" +
                                                 $"{SteamRegistyPath}.{SteamGamePathTag}");
        }

        private readonly HardcodedGameLocationProvider _hardcoded = new HardcodedGameLocationProvider(new[]
        {
            @"C:\Program Files (x86)\Steam\steamapps\common\FINAL FANTASY VIII",//Data\lang-en
            @"D:\SteamLibrary\steamapps\common\FINAL FANTASY VIII",
            @"D:\Steam\steamapps\common\FINAL FANTASY VIII",
        });
    }
}