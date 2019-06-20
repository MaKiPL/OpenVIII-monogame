using System;

namespace OpenVIII
{
    public static class BuildEnvironment
    {
        public static Boolean IsWindows
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