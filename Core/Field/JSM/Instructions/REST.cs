namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class REST : JsmInstruction
    {
        #region Constructors

        public REST()
        {
        }

        public REST(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(REST)}()";

        #endregion Methods
    }
}