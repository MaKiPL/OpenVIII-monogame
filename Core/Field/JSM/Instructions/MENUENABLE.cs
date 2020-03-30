namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class MENUENABLE : JsmInstruction
    {
        #region Constructors

        public MENUENABLE()
        {
        }

        public MENUENABLE(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(MENUENABLE)}()";

        #endregion Methods
    }
}