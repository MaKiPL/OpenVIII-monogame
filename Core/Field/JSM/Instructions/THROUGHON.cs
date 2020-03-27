namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class THROUGHON : JsmInstruction
    {
        #region Constructors

        public THROUGHON()
        {
        }

        public THROUGHON(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(THROUGHON)}()";

        #endregion Methods
    }
}