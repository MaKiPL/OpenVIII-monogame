namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class BATTLERESULT : JsmInstruction
    {
        #region Constructors

        public BATTLERESULT()
        {
        }

        public BATTLERESULT(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(BATTLERESULT)}()";

        #endregion Methods
    }
}