namespace OpenVIII
{
    public static class InitDebuggerBattle
    {
        #region Methods

        public static void Init()
        {
            Memory.Log.WriteLine($"{nameof(InitDebuggerBattle)} :: {nameof(Init)}");
            ArchiveBase aw = ArchiveWorker.Load(Memory.Archives.A_BATTLE);
            byte[] sceneOut = aw.GetBinaryFile("scene.out");
            Memory.Encounters = Battle.Encounters.Read(sceneOut);
            Battle.Mag.Init();
            //Memory.Encounters.CurrentIndex = 87;
        }

        #endregion Methods
    }
}