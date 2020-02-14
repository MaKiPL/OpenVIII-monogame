namespace OpenVIII.Dat_Dump
{

    internal class Program
    {
        
        #region Methods


        private static void Main(string[] args)
        {
            Memory.Init(null, null, null);
            //DumpMonsterAndCharacterDat.Process();
            DumpEncounterInfo.Process();
            
        }


        #endregion Methods
    }
}