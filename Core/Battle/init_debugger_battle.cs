using System.IO;

namespace OpenVIII
{
    public static partial class Init_debugger_battle
    {

        public static void Init()
        {
            Memory.Log.WriteLine($"{nameof(Init_debugger_battle)} :: {nameof(Init)}");
            ArchiveWorker aw = new ArchiveWorker(Memory.Archives.A_BATTLE);
            byte[] sceneOut = aw.GetBinaryFile("scene.out");
            Memory.Encounters = Battle.Encounters.Read(sceneOut);
            Memory.Encounters.CurrentIndex = 87;
        }
    }
}