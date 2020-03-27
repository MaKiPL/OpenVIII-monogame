namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class SHAKEOFF : JsmInstruction
    {
        #region Constructors

        public SHAKEOFF()
        {
        }

        public SHAKEOFF(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(SHAKEOFF)}()";

        #endregion Methods
    }
}