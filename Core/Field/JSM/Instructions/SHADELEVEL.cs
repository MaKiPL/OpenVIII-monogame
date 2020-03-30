namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Shade Level; Sets some shading for the actor.
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/0AF_SHADELEVEL"/>
    public sealed class SHADELEVEL : JsmInstruction
    {
        #region Fields

        private readonly IJsmExpression _arg0;

        #endregion Fields

        #region Constructors

        public SHADELEVEL(IJsmExpression arg0) => _arg0 = arg0;

        public SHADELEVEL(int parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(SHADELEVEL)}({nameof(_arg0)}: {_arg0})";

        #endregion Methods
    }
}