using System;

namespace FF8
{
    public sealed class GameLocation
    {
        public String DataPath { get; }

        public GameLocation(String dataPath)
        {
            DataPath = dataPath;
        }

        public static GameLocation Current { get; } = GetCurrentLocation();

        private static GameLocation GetCurrentLocation()
        {
            IGameLocationProvider provider = GetLocationProvider();
            return provider.GetGameLocation();
        }

        private static IGameLocationProvider GetLocationProvider()
        {
            switch (RuntimeEnvironment.Platform)
            {
                case RuntimePlatform.Windows:
                    return new WindowsGameLocationProvider();
                case RuntimePlatform.Linux:
                    return new LinuxGameLocationProvider();
                default:
                    throw new NotSupportedException(RuntimeEnvironment.Platform.ToString());
            }
        }
    }
}