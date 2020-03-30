namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class FADESYNC : JsmInstruction
    {
        #region Constructors

        public FADESYNC()
        {
        }

        public FADESYNC(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(FADESYNC)}()";

        #endregion Methods
    }
}