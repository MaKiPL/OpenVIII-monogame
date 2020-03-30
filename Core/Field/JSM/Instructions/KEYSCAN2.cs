namespace OpenVIII.Fields.Scripts.Instructions
{
    public sealed class KEYSCAN2 : Abstract.KEY
    {
        #region Constructors

        public KEYSCAN2(KeyFlags flags) : base(flags)
        {
        }

        public KEYSCAN2(int parameter, IStack<IJsmExpression> stack) : base(parameter, stack)
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(KEYSCAN2)}({nameof(_flags)}: {_flags})";

        #endregion Methods
    }
}