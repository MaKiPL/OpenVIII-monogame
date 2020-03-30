namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class MAPJUMPON : JsmInstruction
    {
        #region Constructors

        public MAPJUMPON()
        {
        }

        public MAPJUMPON(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(MAPJUMPON)}()";

        #endregion Methods
    }
}