namespace OpenVIII.Fields.Scripts.Instructions
{
    public interface IJumpToOpcode : IJumpToInstruction
    {
        #region Properties

        int Offset { get; }

        #endregion Properties
    }
}