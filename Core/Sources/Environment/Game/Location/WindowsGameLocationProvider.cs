using Microsoft.Win32;
using System.IO;

namespace OpenVIII
{
    public sealed class WindowsGameLocationProvider : IGameLocationProvider
    {
        /// <summary>
        /// Steam 2013
        /// </summary>
        private const string SteamRegistyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 39150";

        /// <summary>
        /// Steam 2013
        /// </summary>
        private const string SteamGamePathTag = @"InstallLocation";

        /// <summary>
        /// Supplied from LordUrQuan
        /// </summary>
        private const string CD2000RegistyPath = @"SOFTWARE\Wow6432Node\Square Soft, Inc\FINAL FANTASY VIII\1.00";

        /// <summary>
        /// Supplied from LordUrQuan
        /// </summary>
        private const string CD2000GamePathTag = @"AppPath";

        /// <summary>
        /// ReMaster
        /// </summary>
        private const string SteamRERegistyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 1026680";

        /// <summary>
        /// ReMaster
        /// </summary>
        private const string SteamREGamePathTag = @"InstallLocation";

        public GameLocation GetGameLocation()
        {
            if (_hardcoded.FindGameLocation(out GameLocation gameLocation))
                return gameLocation;

            foreach (RegistryView registryView in new RegistryView[] { RegistryView.Registry32, RegistryView.Registry64 })
            {
                GameLocation r;
                if ((r = Check(SteamRegistyPath, SteamGamePathTag)) == null &&
                    (r = Check(CD2000RegistyPath, CD2000RegistyPath)) == null &&
                    (r = Check(SteamRERegistyPath, SteamREGamePathTag)) == null)
                    continue;
                else
                    return r;

                GameLocation Check(string path, string tag)
                {
                    using (RegistryKey localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, registryView))
                    using (RegistryKey registryKey = localMachine.OpenSubKey(path))
                    {
                        if (registryKey != null)
                        {
                            string installLocation = (string)registryKey.GetValue(tag);
                            string dataPath = installLocation;
                            if (Directory.Exists(dataPath))
                                return new GameLocation(dataPath);
                        }
                    }
                    return null;
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