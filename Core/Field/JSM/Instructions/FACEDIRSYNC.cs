namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class FACEDIRSYNC : JsmInstruction
    {
        #region Constructors

        public FACEDIRSYNC()
        {
        }

        public FACEDIRSYNC(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(FACEDIRSYNC)}()";

        #endregion Methods
    }
}