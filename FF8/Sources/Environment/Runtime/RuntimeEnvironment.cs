using System;
using System.Runtime.InteropServices;

namespace FF8
{
    internal static class RuntimeEnvironment
    {
        internal static RuntimePlatform Platform { get; } = Init();

        private static RuntimePlatform Init()
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
                    return GetUnixPlatform();
                case PlatformID.MacOSX:
                    return RuntimePlatform.MacOSX;
                default:
                    throw new NotSupportedException($"Environment.OSVersion.Platform = {platform}");
            }
        }

        private static RuntimePlatform GetUnixPlatform()
        {
            IntPtr buffer = IntPtr.Zero;
            try
            {
                buffer = Marshal.AllocHGlobal(8192);
                if (uname(buffer) == 0)
                {
                    if (Marshal.PtrToStringAnsi(buffer) == "Darwin")
                        return RuntimePlatform.MacOSX;
                }
            }
            catch
            {
                // Nothing
            }
            finally
            {
                if (buffer != IntPtr.Zero)
                    Marshal.FreeHGlobal(buffer);
            }

            return RuntimePlatform.Linux;
        }

        [DllImport("libc")]
        private static extern Int32 uname(IntPtr buf);
    }
}