using System;

namespace FF8
{
    internal static class BuildEnvironment
    {
        internal static Boolean IsWindows
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