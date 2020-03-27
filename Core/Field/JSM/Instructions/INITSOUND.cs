namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class INITSOUND : JsmInstruction
    {
        #region Constructors

        public INITSOUND()
        {
        }

        public INITSOUND(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(INITSOUND)}()";

        #endregion Methods
    }
}