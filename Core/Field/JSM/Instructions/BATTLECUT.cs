namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class BATTLECUT : JsmInstruction
    {
        #region Constructors

        public BATTLECUT()
        {
        }

        public BATTLECUT(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(BATTLECUT)}()";

        #endregion Methods
    }
}