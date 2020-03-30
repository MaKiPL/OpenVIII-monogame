namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class KILLTIMER : JsmInstruction
    {
        #region Constructors

        public KILLTIMER()
        {
        }

        public KILLTIMER(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(KILLTIMER)}()";

        #endregion Methods
    }
}