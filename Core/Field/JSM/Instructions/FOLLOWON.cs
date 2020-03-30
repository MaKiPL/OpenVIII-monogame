namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class FOLLOWON : JsmInstruction
    {
        #region Constructors

        public FOLLOWON()
        {
        }

        public FOLLOWON(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(FOLLOWON)}()";

        #endregion Methods
    }
}