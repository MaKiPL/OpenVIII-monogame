namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class BGOFF : JsmInstruction
    {
        #region Constructors

        public BGOFF()
        {
        }

        public BGOFF(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(BGOFF)}()";

        #endregion Methods
    }
}