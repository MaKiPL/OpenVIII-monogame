namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Set Volume of all Sound Effects
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/0C3_ALLSEVOL"/>
    public sealed class ALLSEVOL : JsmInstruction
    {
        #region Fields

        /// <summary>
        /// Volume (0-127)
        /// </summary>
        private readonly IJsmExpression _arg0;

        #endregion Fields

        #region Constructors

        public ALLSEVOL(IJsmExpression arg0) => _arg0 = arg0;

        public ALLSEVOL(int parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(ALLSEVOL)}({nameof(_arg0)}: {_arg0})";

        #endregion Methods
    }
}