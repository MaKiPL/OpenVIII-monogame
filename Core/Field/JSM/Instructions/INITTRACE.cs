namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class INITTRACE : JsmInstruction
    {
        #region Constructors

        public INITTRACE()
        {
        }

        public INITTRACE(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(INITTRACE)}()";

        #endregion Methods
    }
}