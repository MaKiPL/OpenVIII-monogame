namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class MENUPHS : JsmInstruction
    {
        #region Constructors

        public MENUPHS()
        {
        }

        public MENUPHS(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(MENUPHS)}()";

        #endregion Methods
    }
}