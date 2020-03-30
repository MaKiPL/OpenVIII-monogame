namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class FOOTSTEPOFFALL : JsmInstruction
    {
        #region Constructors

        public FOOTSTEPOFFALL()
        {
        }

        public FOOTSTEPOFFALL(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(FOOTSTEPOFFALL)}()";

        #endregion Methods
    }
}