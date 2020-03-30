namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Transition Pan for all Sound Effects, (Only Used In Test area)
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/0C6_ALLSEPOSTRANS"/>

    public sealed class ALLSEPOSTRANS : JsmInstruction
    {
        #region Fields

        private readonly IJsmExpression _arg0;
        private IJsmExpression _arg1;
        private IJsmExpression _arg2;

        #endregion Fields

        #region Constructors

        public ALLSEPOSTRANS(IJsmExpression arg0, IJsmExpression arg1, IJsmExpression arg2) => _arg0 = arg0;

        public ALLSEPOSTRANS(int parameter, IStack<IJsmExpression> stack)
                : this(
                    arg2: stack.Pop(),
                    arg1: stack.Pop(),
                    arg0: stack.Pop())
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(ALLSEPOS)}({nameof(_arg0)}: {_arg0},{nameof(_arg1)}: {_arg1},{nameof(_arg2)}: {_arg2})";

        #endregion Methods
    }
}