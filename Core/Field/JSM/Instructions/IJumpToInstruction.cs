namespace OpenVIII.Fields.Scripts.Instructions
{
    public interface IJumpToInstruction : IJsmInstruction
    {
        #region Properties

        int Index { get; set; }

        #endregion Properties
    }
}