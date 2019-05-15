using System.Collections.Generic;

namespace FF8.MonoGame
{
    internal static class MonoGameHooks
    {
        internal static void Initialize()
        {
            foreach (IMonoGameHook hook in EnumerateHooks())
                hook.Initialize();
        }

        private static IEnumerable<IMonoGameHook> EnumerateHooks()
        {
            if (RuntimeEnvironment.Platform == RuntimePlatform.Windows)
            {
                // bug fix: NoAudioHardwareException (0x80004005): OpenAL device could not be initialized - NullReferenceException
                yield return new OpenALWindowsHook();
            }
        }
    }
}