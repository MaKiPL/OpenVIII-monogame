namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class MENUDISABLE : JsmInstruction
    {
        #region Constructors

        public MENUDISABLE()
        {
        }

        public MENUDISABLE(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(MENUDISABLE)}()";

        #endregion Methods
    }
}