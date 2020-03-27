namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class LINEON : JsmInstruction
    {
        #region Constructors

        public LINEON()
        {
        }

        public LINEON(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(LINEON)}()";

        #endregion Methods
    }
}