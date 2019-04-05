using System;

namespace FF8
{
    public static class RuntimeEnvironment
    {
        public static RuntimePlatform Platform
        {
            get
            {
                PlatformID platform = Environment.OSVersion.Platform;
                switch (platform)
                {
                    case PlatformID.Win32S:
                    case PlatformID.Win32Windows:
                    case PlatformID.Win32NT:
                    case PlatformID.WinCE:
                        return RuntimePlatform.Windows;
                    case PlatformID.Unix:
                    case (PlatformID)128:
                        return RuntimePlatform.Linux;
                    default:
                        throw new NotSupportedException($"Environment.OSVersion.Platform = {platform}");
                }
            }
        }
    }
}