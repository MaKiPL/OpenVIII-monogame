namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class SCROLLSYNC : JsmInstruction
    {
        #region Constructors

        public SCROLLSYNC()
        {
        }

        public SCROLLSYNC(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(SCROLLSYNC)}()";

        #endregion Methods
    }
}