namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class SETODIN : JsmInstruction
    {
        #region Constructors

        public SETODIN()
        {
        }

        public SETODIN(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(SETODIN)}()";

        #endregion Methods
    }
}