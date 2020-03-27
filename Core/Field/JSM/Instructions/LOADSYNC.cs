namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class LOADSYNC : JsmInstruction
    {
        #region Constructors

        public LOADSYNC()
        {
        }

        public LOADSYNC(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(LOADSYNC)}()";

        #endregion Methods
    }
}