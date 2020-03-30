namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Add Gil
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/151_ADDGIL&action=edit&redlink=1"/>
    public sealed class ADDGIL : JsmInstruction
    {
        #region Fields

        /// <summary>
        /// Ammount of gil?
        /// </summary>
        private readonly IJsmExpression _arg0;

        #endregion Fields

        #region Constructors

        public ADDGIL(IJsmExpression arg0) => _arg0 = arg0;

        public ADDGIL(int parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(ADDGIL)}({nameof(_arg0)}: {_arg0})";

        #endregion Methods
    }
}