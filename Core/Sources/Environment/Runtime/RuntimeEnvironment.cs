using System;
using System.Runtime.InteropServices;

namespace OpenVIII
{
    public static class RuntimeEnvironment
    {
        public static RuntimePlatform Platform { get; } = Init();

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
                if (UnmanagedApi.uname(buffer) == 0)
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
        // Violates rule: MovePInvokesToNativeMethodsClass.
        internal class UnmanagedApi
        {
            [DllImport("libc")]
            internal static extern Int32 uname(IntPtr buf);
        }
    }
}