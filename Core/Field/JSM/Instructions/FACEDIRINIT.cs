namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class FACEDIRINIT : JsmInstruction
    {
        #region Constructors

        public FACEDIRINIT()
        {
        }

        public FACEDIRINIT(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(FACEDIRINIT)}()";

        #endregion Methods
    }
}