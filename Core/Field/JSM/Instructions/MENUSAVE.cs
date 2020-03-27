namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class MENUSAVE : JsmInstruction
    {
        #region Constructors

        public MENUSAVE()
        {
        }

        public MENUSAVE(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(MENUSAVE)}()";

        #endregion Methods
    }
}