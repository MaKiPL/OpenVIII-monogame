using System.IO;

namespace OpenVIII
{
    public sealed class LinuxGameLocationProvider : IGameLocationProvider
    {
        public GameLocation GetGameLocation()
        {
            if (_hardcoded.FindGameLocation(out var gameLocation))
                return gameLocation;

            throw new DirectoryNotFoundException($"Cannot find game directory." +
                                                 $"Add your own path to the {nameof(LinuxGameLocationProvider)} type.");
        }

        private readonly HardcodedGameLocationProvider _hardcoded = new HardcodedGameLocationProvider(new[]
        {
            "/home/robert/Final Fantasy VIII",
            "/media/griever/Data/SteamLibrary/steamapps/common/FINAL FANTASY VIII",
            "/home/griever/.PlayOnLinux/wineprefix/Steam/drive_c/Program Files/Steam/steamapps/common/FINAL FANTASY VIII"
        });
    }
}