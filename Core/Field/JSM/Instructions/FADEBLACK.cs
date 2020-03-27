namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class FADEBLACK : JsmInstruction
    {
        #region Constructors

        public FADEBLACK()
        {
        }

        public FADEBLACK(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(FADEBLACK)}()";

        #endregion Methods
    }
}