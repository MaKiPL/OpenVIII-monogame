namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Transition Volume of all Sound Effects
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/0C4_ALLSEVOLTRANS"/>
    public sealed class ALLSEVOLTRANS : JsmInstruction
    {
        #region Fields

        /// <summary>
        /// Final Volume (0-127)
        /// </summary>
        private readonly IJsmExpression _arg0;

        /// <summary>
        /// Frame Count
        /// </summary>
        private readonly IJsmExpression _arg1;

        #endregion Fields

        #region Constructors

        public ALLSEVOLTRANS(IJsmExpression arg0, IJsmExpression arg1)
        {
            _arg0 = arg0;
            _arg1 = arg1;
        }

        public ALLSEVOLTRANS(int parameter, IStack<IJsmExpression> stack)
            : this(
                arg1: stack.Pop(),
                arg0: stack.Pop())
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(ALLSEVOLTRANS)}({nameof(_arg0)}: {_arg0}, {nameof(_arg1)}: {_arg1})";

        #endregion Methods
    }
}