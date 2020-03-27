namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class FOLLOWOFF : JsmInstruction
    {
        #region Constructors

        public FOLLOWOFF()
        {
        }

        public FOLLOWOFF(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(FOLLOWOFF)}()";

        #endregion Methods
    }
}