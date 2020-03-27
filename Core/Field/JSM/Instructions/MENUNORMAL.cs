namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class MENUNORMAL : JsmInstruction
    {
        #region Constructors

        public MENUNORMAL()
        {
        }

        public MENUNORMAL(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(MENUNORMAL)}()";

        #endregion Methods
    }
}