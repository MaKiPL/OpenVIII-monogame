namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// This has something to do with being in a ladder animation when entering an area. It's used only in two MD Level areas when it detects you've come from another area via ladder.
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/180_UNKNOWN15"/>
    public sealed class Unknown15 : JsmInstruction
    {
        #region Fields

        /// <summary>
        /// 29 or 30
        /// </summary>
        private readonly IJsmExpression _arg0;

        #endregion Fields

        #region Constructors

        public Unknown15(IJsmExpression arg0) => _arg0 = arg0;

        public Unknown15(int parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(Unknown15)}({nameof(_arg0)}: {_arg0})";

        #endregion Methods
    }
}