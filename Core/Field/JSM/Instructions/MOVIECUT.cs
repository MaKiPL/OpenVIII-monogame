namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class MOVIECUT : JsmInstruction
    {
        #region Constructors

        public MOVIECUT()
        {
        }

        public MOVIECUT(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(MOVIECUT)}()";

        #endregion Methods
    }
}