using System.Collections.Generic;

namespace OpenVIII.MonoGame
{
    public static class MonoGameHooks
    {
        public static void Initialize()
        {
            foreach (var hook in EnumerateHooks())
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