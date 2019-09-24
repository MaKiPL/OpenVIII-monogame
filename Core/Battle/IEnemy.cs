namespace OpenVIII
{
    public interface IEnemy : IDamageable
    {
        #region Properties
        byte AP { get; }
        Cards.ID Card { get; }
        Debug_battleDat.Magic[] DrawList { get; }
        Saves.Item[] DropList { get; }
        byte DropRate { get; }
        Module_battle_debug.EnemyInstanceInformation EII { get; set; }
        byte FixedLevel { get; set; }
        Saves.Item[] MugList { get; }
        byte MugRate { get; }
        Kernel_bin.Devour Devour { get; }

        #endregion Properties

        #region Methods

        Saves.Item Drop();
        int EXPExtra(byte lasthitlevel);
        Saves.Item Mug();
        string ToString();

        #endregion Methods
    }
}