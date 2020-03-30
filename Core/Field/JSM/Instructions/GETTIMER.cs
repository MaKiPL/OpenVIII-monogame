namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class GETTIMER : JsmInstruction
    {
        #region Constructors

        public GETTIMER()
        {
        }

        public GETTIMER(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(GETTIMER)}()";

        #endregion Methods
    }
}