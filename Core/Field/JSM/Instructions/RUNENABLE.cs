namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class RUNENABLE : JsmInstruction
    {
        #region Constructors

        public RUNENABLE()
        {
        }

        public RUNENABLE(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(RUNENABLE)}()";

        #endregion Methods
    }
}