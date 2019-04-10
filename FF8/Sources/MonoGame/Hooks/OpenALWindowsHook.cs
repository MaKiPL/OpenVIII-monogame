using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FF8.NetFramework;
using Microsoft.Xna.Framework;

namespace FF8.MonoGame
{
    /*
        This hook loads OpenAL (soft_oal.dll) from the specified location if it wasn't found by the native code.

        Reproduction:
            Try to build and run this project from the directory containing # (e.g. "C:\Git\C#\OpenVIII")

        Cause:
            Incorrectly determined DLL directory.
            Method: MonoGame.OpenAL.AL.GetNativeLibrary (MonoGame.Framework.dll)
            Line: string directoryName = Path.GetDirectoryName(new Uri(typeof (AL).Assembly.CodeBase).LocalPath);
                Uri.OriginalString: file:///C:/Git/C#/OpenVIII/FF8/bin/x64/Debug/MonoGame.Framework.DLL
                Uri.LocalPath: C:\\Git\\C
                Uri.Fragment: #/OpenVIII/FF8/bin/x64/Debug/MonoGame.Framework.DLL
                GetDirectoryName: C:\Git
                Expected: C:\Git\C#\OpenVIII\FF8\bin\x64\Debug

        Exception:       
            Microsoft.Xna.Framework.Audio.NoAudioHardwareException (0x80004005): OpenAL device could not be initialized. ---> System.NullReferenceException:
               at Microsoft.Xna.Framework.Audio.OpenALSoundController.OpenSoundController()
               at Microsoft.Xna.Framework.Audio.OpenALSoundController.OpenSoundController()
               at Microsoft.Xna.Framework.Audio.OpenALSoundController..ctor()
               at Microsoft.Xna.Framework.Audio.OpenALSoundController.get_GetInstance()
               at Microsoft.Xna.Framework.SdlGamePlatform..ctor(Game game)
               at Microsoft.Xna.Framework.Game..ctor()
               at FF8.Game1..ctor()
               at FF8.Program.Main()
     */
    internal sealed class OpenALWindowsHook : IMonoGameHook
    {
        public void Initialize()
        {
            if (RuntimeEnvironment.Platform != RuntimePlatform.Windows)
                return;

            Assembly gameFrameworkDll = typeof(Game).Assembly;

            Type openAL = gameFrameworkDll.RequireType("MonoGame.OpenAL.AL");

            // Invoke static .ctor to avoid overwriting fields
            RuntimeHelpers.RunClassConstructor(openAL.TypeHandle);

            // Get loaded library
            FieldInfo nativeLibraryField = openAL.RequireStaticField("NativeLibrary");
            IntPtr nativeLibrary = (IntPtr)nativeLibraryField.GetValue(obj: /*static*/ null);

            // Return if OpenAL initialized correctly
            if (nativeLibrary != IntPtr.Zero)
                return;

            // Looking for OpenAL (soft_oal.dll)
            nativeLibrary = RequireOpenAL();

            // Overwrite NativeLibrary field
            nativeLibraryField.SetValue(obj: /*static*/ null, value: nativeLibrary);

            // Overwrite Function fields
            foreach (var pair in FieldToFunctionMap)
            {
                String fieldName = pair.Key;
                FieldInfo field = openAL.RequireStaticField(fieldName);

                String functionName = pair.Value;
                Object function = RequireFunction(nativeLibrary, functionName, field.FieldType);

                field.SetValue(obj: /*static*/ null, value: function);
            }
        }

        private static IntPtr RequireOpenAL()
        {
            String platform = Environment.Is64BitProcess ? "x64" : "x86";

            String libraryPath = $"{platform}/soft_oal.dll";
            if (!File.Exists(libraryPath))
                throw new DllNotFoundException($"Cannot find OpenAL DLL: {libraryPath}");

            IntPtr library = WinAPI.LoadLibraryW($"{platform}/soft_oal.dll");
            if (library == IntPtr.Zero)
                throw new Win32Exception();

            return library;
        }

        public static Object RequireFunction(IntPtr library, String function, Type delegateType)
        {
            IntPtr ptr = WinAPI.GetProcAddress(library, function);
            if (ptr == IntPtr.Zero)
                throw new Win32Exception();

            return Marshal.GetDelegateForFunctionPointer(ptr, delegateType);
        }

        private static class WinAPI
        {
            [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern IntPtr LoadLibraryW(String lpszLib);

            [DllImport("kernel32", CharSet = CharSet.Ansi /* GetProcAddress doesn't have an Unicode version. */, SetLastError = true)]
            public static extern IntPtr GetProcAddress(IntPtr hModule, String procName);
        }

        // ReSharper disable StringLiteralTypo
        private static readonly Dictionary<String, String> FieldToFunctionMap = new Dictionary<String, String>
        {
            {"Enable", "alEnable"},
            {"alBufferData", "alBufferData"},
            {"alDeleteBuffers", "alDeleteBuffers"},
            {"Bufferi", "alBufferi"},
            {"GetBufferi", "alGetBufferi"},
            {"Bufferiv", "alBufferiv"},
            {"alGenBuffers", "alGenBuffers"},
            {"alGenSources", "alGenSources"},
            {"GetError", "alGetError"},
            {"alIsBuffer", "alIsBuffer"},
            {"alSourcePause", "alSourcePause"},
            {"alSourcePlay", "alSourcePlay"},
            {"IsSource", "alIsSource"},
            {"alDeleteSources", "alDeleteSources"},
            {"SourceStop", "alSourceStop"},
            {"alSourcei", "alSourcei"},
            {"alSource3i", "alSource3i"},
            {"alSourcef", "alSourcef"},
            {"alSource3f", "alSource3f"},
            {"GetSource", "alGetSourcei"},
            {"GetListener", "alGetListener3f"},
            {"DistanceModel", "alDistanceModel"},
            {"DopplerFactor", "alDopplerFactor"},
            {"alSourceQueueBuffers", "alSourceQueueBuffers"},
            {"alSourceUnqueueBuffers", "alSourceUnqueueBuffers"},
            {"alSourceUnqueueBuffers2", "alSourceUnqueueBuffers"},
            {"alGetEnumValue", "alGetEnumValue"},
            {"IsExtensionPresent", "alIsExtensionPresent"},
            {"alGetProcAddress", "alGetProcAddress"},
            {"alGetString", "alGetString"}
        };
        // ReSharper restore StringLiteralTypo
    }
}