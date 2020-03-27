namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class LINEOFF : JsmInstruction
    {
        #region Constructors

        public LINEOFF()
        {
        }

        public LINEOFF(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(LINEOFF)}()";

        #endregion Methods
    }
}