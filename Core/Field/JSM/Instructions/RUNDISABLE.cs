namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class RUNDISABLE : JsmInstruction
    {
        #region Constructors

        public RUNDISABLE()
        {
        }

        public RUNDISABLE(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(RUNDISABLE)}()";

        #endregion Methods
    }
}