namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class MOVESYNC : JsmInstruction
    {
        #region Constructors

        public MOVESYNC()
        {
        }

        public MOVESYNC(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(MOVESYNC)}()";

        #endregion Methods
    }
}