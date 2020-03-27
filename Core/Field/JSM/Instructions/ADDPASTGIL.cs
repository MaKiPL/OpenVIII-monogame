namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Add Gil to Team Laguna
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/152_ADDPASTGIL&action=edit&redlink=1"/>
    public sealed class ADDPASTGIL : JsmInstruction
    {
        #region Fields

        /// <summary>
        /// Amount of Gil
        /// </summary>
        private readonly IJsmExpression _arg0;

        #endregion Fields

        #region Constructors

        public ADDPASTGIL(IJsmExpression arg0) => _arg0 = arg0;

        public ADDPASTGIL(int parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(ADDPASTGIL)}({nameof(_arg0)}: {_arg0})";

        #endregion Methods
    }
}