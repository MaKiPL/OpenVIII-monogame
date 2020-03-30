namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Trigger Angelo to be disabled or not. Don't know what this does. It uses 1 at the Esthar concourse and the Ragnarok airlock. Uses 0 in the ragnarok cockpit.
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/166_UNKNOWN1"/>
    public sealed class ANGELODISABLE : JsmInstruction
    {
        #region Fields

        /// <summary>
        /// 0 or 1
        /// </summary>
        private readonly bool _angeloDisable;

        #endregion Fields

        #region Constructors

        public ANGELODISABLE(bool angelodisable) => _angeloDisable = angelodisable;

        public ANGELODISABLE(int parameter, IStack<IJsmExpression> stack)
            : this(
                angelodisable: ((IConstExpression)stack.Pop()).Boolean())
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(ANGELODISABLE)}({nameof(_angeloDisable)}: {_angeloDisable})";

        #endregion Methods
    }
}