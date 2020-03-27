namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class FOOTSTEPCUT : JsmInstruction
    {
        #region Constructors

        public FOOTSTEPCUT()
        {
        }

        public FOOTSTEPCUT(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(FOOTSTEPCUT)}()";

        #endregion Methods
    }
}