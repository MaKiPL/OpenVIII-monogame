namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class OFFSETSYNC : JsmInstruction
    {
        #region Constructors

        public OFFSETSYNC()
        {
        }

        public OFFSETSYNC(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(OFFSETSYNC)}()";

        #endregion Methods
    }
}