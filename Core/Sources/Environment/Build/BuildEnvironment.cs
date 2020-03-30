using System;

namespace OpenVIII
{
    public static class BuildEnvironment
    {
        public static bool IsWindows
        {
            get
            {
#if _WINDOWS
                return true;
#else
                return false;
#endif
            }
        }
    }
}