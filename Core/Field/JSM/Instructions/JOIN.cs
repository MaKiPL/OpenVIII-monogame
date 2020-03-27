namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class JOIN : JsmInstruction
    {
        #region Constructors

        public JOIN()
        {
        }

        public JOIN(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(JOIN)}()";

        #endregion Methods
    }
}