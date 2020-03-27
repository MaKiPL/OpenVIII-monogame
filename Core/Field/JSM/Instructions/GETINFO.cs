namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class GETINFO : JsmInstruction
    {
        #region Constructors

        public GETINFO()
        {
        }

        public GETINFO(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(GETINFO)}()";

        #endregion Methods
    }
}