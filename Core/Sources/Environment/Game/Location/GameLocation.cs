using System;

namespace OpenVIII
{
    public sealed class GameLocation
    {
        private readonly static GameLocation s_current = GetCurrentLocation();

        public String DataPath { get; private set; }

        public GameLocation(String dataPath)
        {
            DataPath = dataPath;
        }

        public static GameLocation Current
        {
            get
            { 
                return s_current;
            }
        }

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