namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class FADENONE : JsmInstruction
    {
        #region Constructors

        public FADENONE()
        {
        }

        public FADENONE(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(FADENONE)}()";

        #endregion Methods
    }
}