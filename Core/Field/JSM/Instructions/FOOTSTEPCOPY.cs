namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class FOOTSTEPCOPY : JsmInstruction
    {
        #region Constructors

        public FOOTSTEPCOPY()
        {
        }

        public FOOTSTEPCOPY(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(FOOTSTEPCOPY)}()";

        #endregion Methods
    }
}