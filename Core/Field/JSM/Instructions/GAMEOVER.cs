namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class GAMEOVER : JsmInstruction
    {
        #region Constructors

        public GAMEOVER()
        {
        }

        public GAMEOVER(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(GAMEOVER)}()";

        #endregion Methods
    }
}