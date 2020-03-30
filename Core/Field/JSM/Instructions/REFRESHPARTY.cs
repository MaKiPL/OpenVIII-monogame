namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class REFRESHPARTY : JsmInstruction
    {
        #region Constructors

        public REFRESHPARTY()
        {
        }

        public REFRESHPARTY(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(REFRESHPARTY)}()";

        #endregion Methods
    }
}