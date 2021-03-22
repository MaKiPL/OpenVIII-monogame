using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII.Core {

    /// <summary>
    /// Class is resposible for finding the root directory where Final Fantasy VIII is stored.
    /// </summary>
    public static class GameDirectoryFinder 
    {
        /// <summary>
        /// Looks for root directory where the game is installed.
        /// </summary>
        /// <returns>Path to a directory where the game is installed.</returns>
        public static string FindRootGameDirectory()
        {
            switch(RuntimeEnvironment.Platform)
            {
                case RuntimePlatform.Windows:
                    return WindowsRootGameDirectory();
                case RuntimePlatform.Linux:
                    return LinuxRootGameDirectory();
                default:
                    throw new NotSupportedException(RuntimeEnvironment.Platform.ToString());
            }
        }

        private static string WindowsRootGameDirectory()
        {
            var commonRoots = new string[]
            {
                @"C:\Program Files (x86)\Steam\steamapps\common\FINAL FANTASY VIII",
                @"D:\SteamLibrary\steamapps\common\FINAL FANTASY VIII",
                @"D:\Steam\steamapps\common\FINAL FANTASY VIII",
            };

            if (commonRoots.Where(path => Directory.Exists(path)).Any())
                return commonRoots.Where(path => Directory.Exists(path)).First();

            var registryRoots = RootsFromRegistry();

            if (registryRoots.Where(path => Directory.Exists(path)).Any())
                return registryRoots.Where(path => Directory.Exists(path)).First();

            throw new DirectoryNotFoundException($"Cannot find game directory." +
                                                 $"Add your own path to the {nameof(WindowsRootGameDirectory)}.");
        }

        private static List<string> RootsFromRegistry()
        {
            // Now, we are looking into registers.
            #region Registries paths and tags

            // Steam 2013
            const string SteamRegistryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 39150";
            const string SteamGamePathTag = @"InstallLocation";

            // Supplied from LordUrQuan
            const string CD2000RegistryPath = @"SOFTWARE\Wow6432Node\Square Soft, Inc\FINAL FANTASY VIII\1.00";
            const string CD2000GamePathTag = @"AppPath";

            // Steam Remaster
            const string SteamRERegistryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 1026680";
            const string SteamREGamePathTag = @"InstallLocation";
            #endregion

            var regs = new Dictionary<string, string>()
            {
                {SteamRegistryPath, SteamGamePathTag},
                {CD2000RegistryPath, CD2000GamePathTag},
                {SteamRERegistryPath, SteamREGamePathTag}
            };

            var regValues = new List<string>();
            foreach (var pair in regs)
            {
                regValues.Add(ValueFromRegistry(pair.Key, pair.Value, RegistryView.Registry32));
                regValues.Add(ValueFromRegistry(pair.Key, pair.Value, RegistryView.Registry64));
            }

            return regValues;
        }

        private static string ValueFromRegistry(string subKey, string valueName, RegistryView view)
        {
            using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view);
            using var key = baseKey.OpenSubKey(subKey);

            // Starting from C# 6 we can use Null-conditional operator (?.)
            // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/member-access-operators#null-conditional-operators--and-
            return (string)key?.GetValue(valueName);
        }

        private static string LinuxRootGameDirectory()
        {
            var commonRoots = new string[]
            {
                @"/home/robert/Final Fantasy VIII",
                @"/media/griever/Data/SteamLibrary/steamapps/common/FINAL FANTASY VIII",
                @"/home/griever/.PlayOnLinux/wineprefix/Steam/drive_c/Program Files/Steam/steamapps/common/FINAL FANTASY VIII",
                @"/home/parallels/src/ff8/steam"
            };

            if (commonRoots.Where(path => Directory.Exists(path)).Any())
                return commonRoots.Where(path => Directory.Exists(path)).First();

            throw new DirectoryNotFoundException($"Cannot find game directory." +
                                                 $"Add your own path to the {nameof(LinuxRootGameDirectory)}.");
        }
    }
}