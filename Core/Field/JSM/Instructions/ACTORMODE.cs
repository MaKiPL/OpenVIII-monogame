namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Controls transparency/blinking effects on models (might also control whether Squall/Seifer's gunblades are visible).
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/12D_ACTORMODE"/>
    public sealed class ACTORMODE : JsmInstruction
    {
        #region Fields

        /// <summary>
        /// Model ID?
        /// </summary>
        private readonly IJsmExpression _arg0;

        #endregion Fields

        #region Constructors

        public ACTORMODE(IJsmExpression arg0) => _arg0 = arg0;

        public ACTORMODE(int parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(ACTORMODE)}({nameof(_arg0)}: {_arg0})";

        #endregion Methods
    }
}