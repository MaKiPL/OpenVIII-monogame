namespace OpenVIII
{
    public interface IEnemy : IDamageable
    {
        #region Properties

        byte AP { get; }
        Debug_battleDat.Magic[] DrawList { get; }

        Saves.Item[] DropList { get; }

        byte DropRate { get; }

        Battle.EnemyInstanceInformation EII { get; set; }

        byte FixedLevel { get; set; }

        Saves.Item[] MugList { get; }

        byte MugRate { get; }

        Kernel_bin.Devour Devour { get; }

        #endregion Properties

        #region Methods

        Cards.ID Card();
        Cards.ID CardDrop();

        Saves.Item Drop(bool RareITEM);

        int EXPExtra(byte lasthitlevel);

        Saves.Item Mug(byte spd, bool RareITEM);

        string ToString();

        #endregion Methods
    }
}