namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class MAPJUMPOFF : JsmInstruction
    {
        #region Constructors

        public MAPJUMPOFF()
        {
        }

        public MAPJUMPOFF(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(MAPJUMPOFF)}()";

        #endregion Methods
    }
}